using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using EnvDTE;
using EnvDTE80;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Text;


namespace TRSPOexp.Properties
{

    public partial class ToolWindow1Control : UserControl
    {
        public ToolWindow1Control()
        {
            this.InitializeComponent();
        }
        [SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions", Justification = "Sample code")]
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Default event handler naming pattern")]

        private void AddTabStr(int cnt)
        {
            for (int i = 0; i < cnt; ++i)
                this.table.Items.Add(0);
        }
        private void DelTabRow(int cnt)
        {
            for (int i = 0; i < cnt; ++i)
                table.Items.RemoveAt(0);
        }

        private int GetFunNum(CodeElements elts)
        {
            int FunNum = 0;
            foreach (CodeElement elt in elts)
                if (elt.Kind == vsCMElement.vsCMElementFunction)
                    FunNum++;

            return FunNum;
        }

        private int GetNumAllFuncLines(CodeElement elt)
        {
            return elt.GetEndPoint(vsCMPart.vsCMPartBodyWithDelimiter).Line - elt.GetStartPoint(vsCMPart.vsCMPartHeader).Line + 1;
        }

        private string GetStrFunc(CodeElement elt)
        {
            TextPoint begin = elt.GetStartPoint(vsCMPart.vsCMPartHeader);
            TextPoint end = elt.GetEndPoint(vsCMPart.vsCMPartBodyWithDelimiter);
            string func = begin.CreateEditPoint().GetLines(begin.Line, end.Line + 1);
            return func;
        }


        private static int GoTo(int indent, string s, ref int i, string pattern, string skip = null)
        {
            for (; i < s.Length; i++)
            {
                if (!String.IsNullOrEmpty(skip))
                    while (TryRead(s, ref i, skip)) ;
                if (TryRead(s, ref i, pattern)) break;
            }
            if (indent == 1)
            {
                if (s[i- 3] == '\\')
                {
                    return 1;
                }
            }
            return 0;
        }
        static bool TryRead(string s, ref int pos, string pattern)
        {
            if (pattern.Length == 0) return true;

            for (int i = 0; i < pattern.Length; i++)
                if (pos + i >= s.Length || s[pos + i] != pattern[i])
                    return false;
            pos += pattern.Length;
            return true;
        }
        private static string RemoveCommentsAndStrings(string source)
        {
            int res=1;
            var result = new StringBuilder();
            for (int i = 0; i < source.Length;)
            {
                if (TryRead(source, ref i, "//")) {
                    res = 1;
                    while (res != 0) { 
                        res = GoTo(1, source, ref i, "\r\n");
                        if (res == 1)
                        {
                            res = GoTo(1, source, ref i, "\r\n");
                        }
                    } 
                    result.AppendLine(); }
                else if (TryRead(source, ref i, "/*")) GoTo(2,source, ref i, "*/");
                else if (TryRead(source, ref i, "@\"")) { GoTo(2,source, ref i, "\"", "\"\""); result.Append("\"\""); }
                else if (TryRead(source, ref i, "\"")) { GoTo(2,source, ref i, "\"", "\\\""); result.Append("\"\""); }
                else
                {
                    result.Append(source[i]);
                    i++;
                }
            }
            return result.ToString();
        }



        private int GetNumLines(ref string func)
        {
            func = RemoveCommentsAndStrings(func);
            func = Regex.Replace(func, @"^\s+$[\r\n]*", "", RegexOptions.Multiline);

            int lines = 1;
            foreach (char c in func)
                if (c == '\n') lines++;

            return lines;
        }

        private int NumOfKeyWords(string func)
        {
            if (func == null)
                return 0;
            int res = 0;
            for (int i = 0; i < KeyWords.Length; ++i)
            {
                string pattern = "";
                pattern += KeyWords[i] + "\\W";
                res += Regex.Matches(func, @pattern).Count;
                pattern = null;
                pattern = "(_";
                pattern += KeyWords[i] + "\\W" + ")|(\\w" + KeyWords[i] + "\\W)";
                res -= Regex.Matches(func, @pattern).Count;

            }
            return res;
        }

        string[] KeyWords = { "alignas","alignof","andB","and_eqB","asma",
            "auto","bitandB","bitorB","bool","break","case","catch","char",
            "char8_tc","char16_t","char32_t","class","complB","conceptc",
            "const","const_cast","constevalc","constexpr","constinitc",
            "continue","co_awaitc","co_returnc","co_yieldc","decltype",
            "default","delete","do","double","dynamic_cast","else","enum",
            "explicit","exportc","extern","false","float","for","friend",
            "goto","if","inline","int","long","mutable","namespace","new",
            "noexcept","notB","not_eqB","nullptr","operator","orB","or_eqB",
            "private","protected","public","register","reinterpret_cast",
            "requiresc","return","short","signed","sizeof","static",
            "static_assert","static_cast","struct","switch","template","this",
            "thread_local","throw","true","try","typedef","typeid","typename",
            "union","unsigned","using","virtual","void","volatile","wchar_t",
            "while","xorB","xor_eqB" };
        private void Update(object sender, RoutedEventArgs e)
        {

            DTE2 dte = TRSPOexpPackage.GetGlobalService(typeof(DTE)) as DTE2;
            FileCodeModel fileCM = dte.ActiveDocument.ProjectItem.FileCodeModel;
            CodeElements elts = fileCM.CodeElements;

            int FuncNum = GetFunNum(elts);
            if (table.Items.Count < FuncNum)
                AddTabStr(FuncNum - table.Items.Count);
            else if (table.Items.Count > FuncNum)
                DelTabRow(table.Items.Count - FuncNum);

            string FuncName = null;
            string func = null;
            int FuncCnt = 0;
            int AllFuncLines;
            int Lines;
            int KeyWordsCnt;
            foreach (CodeElement elt in elts)
            {
                if (elt.Kind == vsCMElement.vsCMElementFunction)
                {
                    FuncName = elt.FullName;
                    func = GetStrFunc(elt);
                    AllFuncLines = GetNumAllFuncLines(elt);
                    Lines = GetNumLines(ref func);
                    KeyWordsCnt = NumOfKeyWords(func);
                    table.Items[FuncCnt] = new { Function = FuncName, AllLines = AllFuncLines, LinesWithoutCom = Lines, KeyWords = KeyWordsCnt };
                    FuncCnt++;
                }
            }
            return;
        }

    }
}