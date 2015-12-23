using System.Linq;
using AppsTracker.Service.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AppsTracker.Tests.Core.Service.Web
{
    [TestClass]
    public class ReleaseNotesParserTest
    {
        [TestMethod]
        public void TestJsonParse()
        {
            var parser = new ReleaseNotesParser();
            var json = "[\n{\n\"version_number\": \"3.0.3.0\",\n        \"release_notes\": [    \n\"Fixed an error where limits with action set to warn caused the app to crash.\",\n            \"Minor bugs fixes and stability improvements.\"\n        ]\n    },\n    {\n        \"version_number\": \"3.0.2.0\",\n        \"release_notes\": [\n            \"Fixed a bug when returning from settings caused the app to crash sometimes.\",\n            \"Fixed an error when manually editing the settings xml file stored in the AppData folder caused the app to crash. The file settings.xml can be deleted manually so the app can start again.\"\n        ]\n    },\n    {\n        \"version_number\": \"3.0.1.0\",\n        \"release_notes\": [\n            \"Changing themes now animates the color change.\",\n            \"Fixed an issue where stopping tracking caused the app to crash.\"\n        ]\n    },\n    {\n        \"version_number\": \"3.0.0.0\",\n        \"release_notes\": [\n            \"Added an option to submit feedback or bug reports directly from the app\",\n            \"Release notes can now be displayed from the settings view\"\n        ]\n    }\n]";

            var notes = parser.ParseJson(json);

            Assert.IsNotNull(notes);
            Assert.AreEqual(notes.Count(), 4);

            var version3000 = notes.Single(n => n.Version == "3.0.0.0");

            Assert.AreEqual(1, version3000.Notes.Count(s => s == "Added an option to submit feedback or bug reports directly from the app"));
            Assert.AreEqual(1, version3000.Notes.Count(s => s == "Release notes can now be displayed from the settings view"));

            var version3010 = notes.Single(n => n.Version == "3.0.1.0");

            Assert.AreEqual(1, version3010.Notes.Count(s => s == "Changing themes now animates the color change."));
            Assert.AreEqual(1, version3010.Notes.Count(s => s == "Fixed an issue where stopping tracking caused the app to crash."));

            var version3020 = notes.Single(n => n.Version == "3.0.2.0");

            Assert.AreEqual(1, version3020.Notes.Count(s => s == "Fixed a bug when returning from settings caused the app to crash sometimes."));
            Assert.AreEqual(1, version3020.Notes.Count(s => s == "Fixed an error when manually editing the settings xml file stored in the AppData folder caused the app to crash. The file settings.xml can be deleted manually so the app can start again."));

            var version3030 = notes.Single(n => n.Version == "3.0.3.0");

            Assert.AreEqual(1, version3030.Notes.Count(s => s == "Fixed an error where limits with action set to warn caused the app to crash."));
            Assert.AreEqual(1, version3030.Notes.Count(s => s == "Minor bugs fixes and stability improvements."));
        }
    }
}
