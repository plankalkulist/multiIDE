using NUnit.Framework;

namespace multiIDE.CodeEditors.Test
{
    [TestFixture]
    public class StdCodeEditorTest
    {
        [Test]
        public void Preprocess_WithoutMacros()
        {
            StdCodeEditor stdCodeEditor = new StdCodeEditor();
            string code =
                "+[0>0>>0>, <<, >>[ <<[ #m \"3\":\">>>\"#3[-]+ <[-<+<->>>[-] >>[-] " +
                "<<<[>>>+<<<-]] >>>[<<<+>>>-] <<[ <<<[>>>>[-]+<<<<[-" +
                "]] 3[-]] <<<] >[<+>-] <<+ 3] >>[ <<<<<- >3>-]" +
                " 0<0<0<0<0<.]p ";

            string processedCode = stdCodeEditor.Preprocess(code);

            Assert.That(processedCode, Is.EqualTo(code));
        }

        [Test]
        public void Preprocess_OneMacro()
        {
            StdCodeEditor stdCodeEditor = new StdCodeEditor();
            string code  =
                "+[0>0>>0>, <<, >>[ <<[ #m\"3\":\">>>\"#3[-]+ <[-<+<->>>[-] >>[-] " +
                "<<<[>>>+<<<-]] >>>[<<<+>>>-] <<[ <<<[>>>>[-]+<<<<[-" +
                "]] 3[-]] <<<] >[<+>-] <<+ 3] >>[ <<<<<- >3>-]" +
                " 0<0<0<0<0<.]p ";

            string processedCode = stdCodeEditor.Preprocess(code);
            
            Assert.That(processedCode, Is.EqualTo("+[0>0>>0>, <<, >>[ <<[ >>>[-]+ <[-<+<->>>[-] >>[-] " +
                "<<<[>>>+<<<-]] >>>[<<<+>>>-] <<[ <<<[>>>>[-]+<<<<[-" +
                "]] >>>[-]] <<<] >[<+>-] <<+ >>>] >>[ <<<<<- >>>>>-]" +
                " 0<0<0<0<0<.]p "));
        }

        [Test]
        public void Preprocess_ManyMacros()
        {
            StdCodeEditor stdCodeEditor = new StdCodeEditor();
            string code =
                "+[0>0>>0>, <<, >>[ <<[ #m\"3\":\">>>\"#3[-]+ <[-<+<->>>[-] >>[-] " +
                "<<<[>>>+<<<-]] >>>[<<<#m\"plus\":\"+\"#plus>>>-] <<[ <<<[>>>>[-]plus<<<<[-" +
                "]] 3[-]] <<<] >[<+>-] <<plus 3] >>[ <<3plus <<<- >3>-]" +
                " 0<0<0<0<0<.]p ";

            string processedCode = stdCodeEditor.Preprocess(code);

            Assert.That(processedCode, Is.EqualTo("+[0>0>>0>, <<, >>[ <<[ >>>[-]+ <[-<+<->>>[-] >>[-] " +
                                                  "<<<[>>>+<<<-]] >>>[<<<+>>>-] <<[ <<<[>>>>[-]+<<<<[-" +
                                                  "]] >>>[-]] <<<] >[<+>-] <<+ >>>] >>[ <<>>>+ <<<- >>>>>-]" +
                                                  " 0<0<0<0<0<.]p "));
        }
    }
}
