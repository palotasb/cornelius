using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Cornelius.Grammar;
using Cornelius.IO;
using Cornelius.IO.Mapping;
using Cornelius.IO.Primitives;

namespace Cornelius
{
    class Import
    {
        public Recognizer<XIdentity> Identities
            { get; protected set; }

        public Recognizer<XSpecialization> Specializations
            { get; protected set; }

        public Recognizer<XSpecializationGrouping> SpecializationGroupings
            { get; protected set; }

        public Recognizer<XEntry> Entries
            { get; protected set; }

        public Recognizer<XChoice> Choices
            { get; protected set; }

        public Recognizer<XBase> Exceptions
        { get; protected set; }

        protected Import()
        {
            this.Identities = new Recognizer<XIdentity>(MapDefinitionLanguageParser.Parse(Path.Combine(Program.CONFIG_DIRECTORY, "bemenet-hallgatok.ifd")));
            this.Specializations = new Recognizer<XSpecialization>(MapDefinitionLanguageParser.Parse(Path.Combine(Program.CONFIG_DIRECTORY, "bemenet-specializaciok.ifd")));
            this.SpecializationGroupings = new Recognizer<XSpecializationGrouping>(MapDefinitionLanguageParser.Parse(Path.Combine(Program.CONFIG_DIRECTORY, "bemenet-specializacio-csoportok.ifd")));
            this.Entries = new Recognizer<XEntry>(MapDefinitionLanguageParser.Parse(Path.Combine(Program.CONFIG_DIRECTORY, "bemenet-bejegyzesek.ifd")));
            this.Choices = new Recognizer<XChoice>(MapDefinitionLanguageParser.Parse(Path.Combine(Program.CONFIG_DIRECTORY, "bemenet-valasztasok.ifd")));
            this.Exceptions = new Recognizer<XBase>(MapDefinitionLanguageParser.Parse(Path.Combine(Program.CONFIG_DIRECTORY, "bemenet-kivetelek.ifd")));
        }

        public static Import LoadAllFiles()
        {
            Log.Write("Bemeneti fájlok betöltése...");
            Log.EnterBlock();
            Import import = new Import();
            foreach (var path in Directory.GetFiles(Program.INPUT_DIRECTORY, "*", SearchOption.AllDirectories).Where(path => !path.EndsWith(".kepzes")))
            {
                Log.Write(path);
                Log.EnterBlock(" => ");
                if (import.Identities.Recognize(path))
                {
                    Log.Write("A fájlban hallgatók vannak.");
                    var identities = import.Identities.Parse(path);
                    foreach (var identity in identities)
                    {
                        if (identity.OriginalEducationProgram == null)
                        {
                            identity.OriginalEducationProgram = identity.BaseEducationProgram;
                        }
                    }
                    if (File.Exists(path + ".kepzes"))
                    {
                        string specified = File.ReadAllText(path + ".kepzes").Substring(0, 5);
                        Log.Write("A kurzuskód felülírásra kerül (" + specified + ").");
                        foreach (var identity in identities)
                        {
                            identity.BaseEducationProgram = specified;
                        }
                    }
                }
                else if (import.Entries.Recognize(path))
                {
                    Log.Write("A fájlban tárgybejegyzések vannak.");
                    import.Entries.Parse(path);
                }
                else if (import.Choices.Recognize(path))
                {
                    Log.Write("A fájlban specializációválasztások vannak.");
                    import.Choices.Parse(path);
                }
                else if (import.Specializations.Recognize(path))
                {
                    Log.Write("A fájlban a specializáció-ágazatok adatai vannak.");
                    import.Specializations.Clear();
                    import.Specializations.Parse(path);
                }
                else if (import.SpecializationGroupings.Recognize(path))
                {
                    Log.Write("A fájlban specializációcsoportok adatai vannak.");
                    import.SpecializationGroupings.Clear();
                    import.SpecializationGroupings.Parse(path);
                }
                else if (import.Exceptions.Recognize(path))
                {
                    Log.Write("A fájlban kivételkezelések vannak.");
                    import.Exceptions.Parse(path);
                }
                else
                {
                    Log.Write("Nem adatfájl, vagy nem felismerhető fájlnév vagy formátum.");
                }
                Log.LeaveBlock();
            }
            Log.LeaveBlock();
            return import;
        }
    }
}
