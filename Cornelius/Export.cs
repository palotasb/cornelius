using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Cornelius.Criteria;
using Cornelius.Data;
using Cornelius.IO;
using Cornelius.IO.Reports;
using Cornelius.IO.Drivers;
using Cornelius.IO.Tables;
using Ionic.Zip;

namespace Cornelius
{
    class Export
    {
        protected Export()
        {
            _rows = new List<AbstractRow>();
            _associations = new Dictionary<string, int>();
        }

        protected List<AbstractRow> _rows;
        protected Dictionary<string, int> _associations;

        public static string PATH_USERS_INSERT
        {
            get
            { 
                return Path.Combine(Program.OUTPUT_DIRECTORY, "Adatbázis - Felhasználók (kezdeti).sql");
            }
        }

        public static string PATH_USERS_UPDATE
        {
            get
            {
                return Path.Combine(Program.OUTPUT_DIRECTORY, "Adatbázis - Felhasználók (frissítés).sql");
            }
        }

        public static string PATH_USERS_DIFFERENCE
        {
            get
            {
                return Path.Combine(Program.OUTPUT_DIRECTORY, "Adatbázis - Felhasználók (újak, " + DateTime.Now.ToShortDateString() + ").sql");
            }
        }

        public static string PATH_RESULTS_SQL
        {
            get
            { 
                return Path.Combine(Program.OUTPUT_DIRECTORY, "Adatbázis - Eredmények (nyers).sql");
            }
        }

        public static string PATH_RESULTS_ZIP
        {
            get
            {
                return Path.Combine(Program.OUTPUT_DIRECTORY, "Adatbázis - Eredmények (tömörített).zip");
            }
        }

        public static string PATH_REPORT
        {
            get
            {
                return Path.Combine(Program.OUTPUT_DIRECTORY, "Zárójelentés.xls");
            }
        }

        public static string PATH_HISTOGRAM
        {
            get
            {
                return Path.Combine(Program.OUTPUT_DIRECTORY, "Statisztika.xls");
            }
        }

        public static string PATH_ASSOCIATIONS
        {
            get
            {
                return Path.Combine(Program.OUTPUT_DIRECTORY, "Adatbázis - Neptun hozzárendelés.csv");
            }
        }

        protected void ExportCourses(Result result, XResult parent)
        {
            foreach (var course in result.Courses)
            {
                _rows.Add(new XCourse(parent, course));
            }
        }

        protected void ExportAvarages(Student student, XUser owner)
        {
            foreach (var course in student.Result.Courses.Where(c => c.Credit > 0 || !c.ExcludeFromAverage))
            {
                _rows.Add(new XAvarage(owner, course));
            }

            double missing = student.Result.Credit - student.Result.Courses.Sum(course => course.Credit);
            if (missing > 0)
            {
                _rows.Add(new XAvarage(owner, missing));
            }
        }

        protected void ExportEmails(Student student, XUser owner)
        {
            foreach (var email in student.Emails)
            {
                _rows.Add(new XEmail(owner, email));
            }
        }

        protected void ExportChoices(Student student, XUser owner)
        {
            for (int i = 0; i < student.Choices.Length; ++i)
            {
                _rows.Add(new XChoice(owner, i + 1, student.Choices[i]));
            }
        }

        protected void ExportResults(Result result, XResult parent, XUser owner, int preserveDepth)
        {
            if ((result.Name != null && result.Name.Length > 0 && result.Subresults.Count > 0) || preserveDepth > 0)
            {
                parent = new XResult(owner, parent, result);
                _rows.Add(parent);
            }
            if (result.Subresults.Count == 0 && result.Courses.Count > 0)
            {
                this.ExportCourses(result, parent);
            }
            else
            {
                foreach (var sub in result.Subresults.OrderBy(s => s.Semester == null ? 100 : s.Semester).ThenBy(s => s.Name))
                {
                    this.ExportResults(sub, parent, owner, preserveDepth - 1);
                }
            }
        }

        protected void ExportUser(Student student)
        {
            var user = new XUser(student, _associations.ContainsKey(student.Key) ? (int?)_associations[student.Key] : null);
            _rows.Add(user);

            this.ExportResults(student.Result, null, user, 2);
            this.ExportEmails(student, user);
            this.ExportChoices(student, user);
            this.ExportAvarages(student, user);
        }

        protected void WriteTables()
        {
            File.Delete(Export.PATH_USERS_INSERT);
            File.Delete(Export.PATH_USERS_UPDATE);
            File.Delete(Export.PATH_RESULTS_SQL);
            File.Delete(Export.PATH_RESULTS_ZIP);

            StructuredQueryDriver driver = new StructuredQueryDriver();

            driver.QueryType = StructuredQueryType.TruncateInsert;
            TableWriter.Write<XUser>(Export.PATH_USERS_INSERT, _rows.OfType<XUser>(), driver);
            Log.Write(Export.PATH_USERS_INSERT);

            driver.QueryType = StructuredQueryType.UpdateWithFirstAsKey;
            TableWriter.Write<XUser>(Export.PATH_USERS_UPDATE, _rows.OfType<XUser>(), driver);
            Log.Write(Export.PATH_USERS_UPDATE);

            if (_rows.OfType<XUser>().Any(user => user.IsProvisioned) && !_rows.OfType<XUser>().All(user => user.IsProvisioned))
            {
                driver.QueryType = StructuredQueryType.Insert;
                TableWriter.Write<XUser>(Export.PATH_USERS_DIFFERENCE, _rows.OfType<XUser>().Where(user => user.IsProvisioned), driver);
                Log.Write(Export.PATH_USERS_DIFFERENCE);
            }

            driver.QueryType = StructuredQueryType.TruncateInsert;
            TableWriter.Write<XEmail>(Export.PATH_RESULTS_SQL, _rows.OfType<XEmail>(), driver);
            TableWriter.Write<XChoice>(Export.PATH_RESULTS_SQL, _rows.OfType<XChoice>(), driver);
            TableWriter.Write<XResult>(Export.PATH_RESULTS_SQL, _rows.OfType<XResult>(), driver);
            TableWriter.Write<XCourse>(Export.PATH_RESULTS_SQL, _rows.OfType<XCourse>(), driver);
            TableWriter.Write<XAvarage>(Export.PATH_RESULTS_SQL, _rows.OfType<XAvarage>(), driver);
            Log.Write(Export.PATH_RESULTS_SQL);

            ZipFile zip = new ZipFile(Export.PATH_RESULTS_ZIP);
            zip.CompressionLevel = Ionic.Zlib.CompressionLevel.BestCompression;
            zip.AddFile(Export.PATH_RESULTS_SQL, "");
            zip.Save();
            Log.Write(Export.PATH_RESULTS_ZIP);
        }

        protected void WriteAssociations()
        {
            var driver = new SeparatedTextDriver();
            TableWriter.Write<XAssociation>(Export.PATH_ASSOCIATIONS, _rows.OfType<XUser>().Select(v => new XAssociation(v.Neptun, v.EducationProgram, v.Identifier)), driver);
            Log.Write(Export.PATH_ASSOCIATIONS);
        }

        protected void ReadAssociations()
        {
            var driver = new SeparatedTextDriver();
            if (File.Exists(Export.PATH_ASSOCIATIONS))
            {
                _associations = TableReader.Read<XAssociation>(Export.PATH_ASSOCIATIONS, driver).ToDictionary(ass => ass.Key, ass => ass.Identifier);
                if (_associations.Count > 0)
                {
                    Provision.ContinueFrom(typeof(XUser), _associations.Max(pair => pair.Value));
                }
            }
        }

        public static void ExportDatabases(IEnumerable<Student> students)
        {
            Export e = new Export();
            Provision.Clear();
            e.ReadAssociations();
            foreach (var student in students)
            {
                e.ExportUser(student);
            }
            e.WriteAssociations();
            e.WriteTables();
        }

        delegate bool StatisticalFilter(Student s);

        public static void ExportCreditStatistics(IEnumerable<Student> students)
        {
            Dictionary<string, StatisticalFilter> filters = new Dictionary<string, StatisticalFilter>();
            filters["MIND"] = s => true;
            filters["PASS"] = s => s.Specialization != null;
            filters["FAIL"] = s => s.Specialization == null;
            foreach (var code in students.Select(s => s.EducationProgram).Distinct())
            {
                foreach (var filter in filters)
                {
                    Dictionary<string, XHistogram[]> histograms = new Dictionary<string, XHistogram[]>();
                    string[] groups = students.Where(s => s.EducationProgram == code).SelectMany(s => s.CreditPerGroup.Keys).Distinct().ToArray();
                    foreach (string group in groups)
                    {
                        var values = students.Where(s => s.EducationProgram == code).Where(s => filter.Value(s)).Where(s => s.CreditPerGroup.Sum(e => e.Value) > 0).Select(student => student.CreditPerGroup[group]);
                        var histogram = values.GroupBy(i => i).OrderBy(e => e.Key).Select(e => new XHistogram(e.Key, e.Count()));
                        histograms.Add(group, histogram.ToArray());
                    }
                    var driver = new ExcelDriver();
                    foreach (var sheet in histograms)
                    {
                        TableWriter.Write<XHistogram>(Export.PATH_HISTOGRAM, sheet.Value, driver, code + "+" + filter.Key + "+" + sheet.Key);
                    }
                }
            } 
            Log.Write(Export.PATH_HISTOGRAM);
        }

        public static void ExportReports(IEnumerable<Student> students, IEnumerable<Specialization> specializations, IEnumerable<SpecializationGrouping> specializationGroupings)
        {
            var driver = new ExcelDriver();
            TableWriter.Write<XStudent>(Export.PATH_REPORT, students.Select(student => new XStudent(student)), driver);
            TableWriter.Write<XSumma>(Export.PATH_REPORT, specializations.Select(specialization =>
                new XSumma(specialization, specializationGroupings)), driver);
            Log.Write(Export.PATH_REPORT);
        }
    }
}
