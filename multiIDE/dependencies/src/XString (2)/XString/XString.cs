namespace XSpace
{
    public static class XString
    {
        public const int xsNA = -1;

        public enum TextAlignment
        {
            None,
            Left,
            Center,
            Right
        }

        public static string Left(this string expression, int length) => expression.Substring(0, length);
        public static string Right(this string expression, int length) => expression.Substring(expression.Length - length);
        public static bool IsNotNullOrEmpty(this string s) => s != null && s != "";

        public static string bMid(this byte[] expression, int start, int length = -1)
        {
            //обработка аргументов
            if (start < 0 || expression.Length < 1 || start + length > expression.Length) return "";
            if (length == 0) return "";
            if (length == -1) length = expression.Length - start;
            //
            string s = "";

            for (int a = start; a < start + length; a++)
                s += (char)(expression[a]);

            return s;
        }

        public static int CountLevel(this string expression, int start, int length = -1, string ups = "", string downs = "")
        {
            //обработка аргументов
            if (start + length - 1 > expression.Length)
                return xsNA;
            if (string.IsNullOrEmpty(ups) || string.IsNullOrEmpty(downs))
                return xsNA;
            if (length == -1) length = expression.Length - start;
            //
            int A = 0;
            int B = 0;
            char M = '\0';
            int L = 0;
            bool N = false;
            L = 0;

            for (A = start; A < start + length; A++)
            {
                N = false;
                M = expression[A];
                for (B = 0; B < ups.Length; B++)
                {
                    if (M == ups[B])
                    {
                        L += 1; N = true; break; // TODO: might not be correct. Was : Exit For
                    }
                }
                if (N)
                    continue;
                for (B = 0; B < downs.Length; B++)
                {
                    if (M == downs[B])
                    {
                        L -= 1; break; // TODO: might not be correct. Was : Exit For
                    }
                }
            }

            return L;
        }

        public static int CountLevel(this byte[] expression, int start, int length = -1, string ups = "", string downs = "")
        {
            //обработка аргументов
            if (start + length - 1 > expression.Length)
                return xsNA;
            if (string.IsNullOrEmpty(ups) || string.IsNullOrEmpty(downs))
                return xsNA;
            if (length == -1) length = expression.Length - start;
            //
            int A = 0;
            int B = 0;
            char M = '\0';
            int L = 0;
            bool N = false;
            L = 0;

            for (A = start; A < start + length; A++)
            {
                N = false;
                M = (char)expression[A];
                for (B = 0; B < ups.Length; B++)
                {
                    if (M == ups[B])
                    {
                        L += 1; break; // TODO: might not be correct. Was : Exit For
                    }
                }
                if (N)
                    continue;
                for (B = 0; B < downs.Length; B++)
                {
                    if (M == downs[B])
                    {
                        L -= 1; break; // TODO: might not be correct. Was : Exit For
                    }
                }
            }

            return L;
        }

        public static int FindClosing(this byte[] expression, int start, int length = -1,
                                    char opening = '\0', char closing = '\0',
                                    string matchWith = "", string missingOpening = "",
                                    string missingClosing = "")
        {
            //обработка аргументов
            if (start + length - 1 > expression.Length)
                return xsNA;
            if (opening == '\0' || closing == '\0')
                return xsNA;
            if (length == -1) length = expression.Length - start;
            //
            int ClosingPosition = xsNA;
            int A = 0;
            int B = 0;
            char M = '\0';
            int L = 1;

            if (closing != opening)
            {
                for (A = start; A < start + length; A++)
                {
                    M = (char)expression[A];
                    if (M == opening)
                    {
                        L += 1;
                    }
                    else if (M == closing)
                    {
                        L -= 1;
                        if (L == 0) { ClosingPosition = A; break; }
                    }
                    else
                    {
                        for (B = 0; B < matchWith.Length; B++)
                        {
                            if (M == matchWith[B]) { L += 1; goto CONT; }
                        }
                        for (B = 0; B < missingOpening.Length; B++)
                        {
                            if (M == missingOpening[B])
                            {
                                A = FindClosing(expression, A + 1, length - (A - start + 1), missingOpening[B], missingClosing[B]);
                                goto CONT;
                            }
                        }
                    }
                CONT: B = 0; // NOP, continue
                }
            }
            else
            {
                for (A = start + 1; A < start + length; A++)
                {
                    if ((char)(expression[A]) == closing)
                    {
                        ClosingPosition = A; break; // TODO: might not be correct. Was : Exit For
                    }
                }
            }

            return ClosingPosition;
        }

        public static void ReplaceChars(ref string expression, int start, int length = -1, string find = "", string replace = "")
        {
            //обработка аргументов
            if (start + length - 1 > expression.Length)
                return;
            if (find.Length != replace.Length || string.IsNullOrEmpty(find) || string.IsNullOrEmpty(replace))
                return;
            if (length == -1) length = expression.Length - start;
            //
            string s = expression.Substring(start, length);
            for (int B = 0; B < find.Length; B++)
            {
                s = s.Replace(find[B], replace[B]);
            }
            expression = expression.Remove(start, length);
            expression = expression.Insert(start, s);
        }

        public static int ReplaceChars(byte[] expression, int start, int length = -1, string find = "", string replace = "")
        {
            //обработка аргументов
            if (start + length - 1 > expression.Length)
                return xsNA;
            if (find.Length != replace.Length || string.IsNullOrEmpty(find) || string.IsNullOrEmpty(replace))
                return xsNA;
            if (length == -1) length = expression.Length - start;
            //
            int A = 0;
            int B = 0;
            char M = '\0';
            int L = 0;
            byte[] bF = new byte[find.Length];
            byte[] bR = new byte[replace.Length];

            // ERROR: Not supported in C#: ReDimStatusment

            for (A = 0; A < find.Length; A++)
            {
                bF[A] = (byte)find[A];
            }

            for (A = 0; A < replace.Length; A++)
            {
                bR[A] = (byte)replace[A];
            }

            for (A = start; A < start + length; A++)
            {
                M = (char)(expression[A]);
                for (B = 0; B < find.Length; B++)
                {
                    if (M == find[B])
                    {
                        expression[A] = (byte)replace[B];
                        L += 1;
                        break; // TODO: might not be correct. Was : Exit For
                    }
                }
            }

            return L;
        }

        public static string Spaces(int number)
        {
            string s = "";

            for (int A = 0; A < number; A++)
                s += " ";

            return s;
        }

        public static string DupCharToString(char character, int length)
        {
            string s = "";

            for (int i = 0; i < length; i++)
                s += character;

            return s;
        }

        public static string ShrinkFileName(this string fileFullName, int maxLength, TextAlignment alignment = TextAlignment.None, char padding = ' ')
        {
            if (string.IsNullOrEmpty(fileFullName))
                return "";

            string[] pathTerms = fileFullName.Split('\\');
            string last = "";
            string leftCurrent = "";
            string rightCurrent = "";
            string result;
            int direction = 0;
            int leftProceeded = 0;
            int rightProceeded = 0;

            if (maxLength < 1 || pathTerms == null || pathTerms.Length < 1)
                return "";
            else if (maxLength < pathTerms[pathTerms.Length - 1].Length)
                last = "Е";
            else if (pathTerms.Length == 1)
                last = pathTerms[0];
            else if (maxLength >= fileFullName.Length)
                last = fileFullName;
            else
            {

                do
                {
                    last = leftCurrent + "\\Е\\" + rightCurrent;
                    direction++;
                    switch (direction)
                    {
                        case 1:
                            rightProceeded++;
                            rightCurrent = pathTerms[pathTerms.Length - 1];
                            break;
                        case 2:
                            rightProceeded++;
                            rightCurrent = pathTerms[pathTerms.Length - 2] + "\\" + rightCurrent;
                            break;
                        case 3:
                            leftCurrent = pathTerms[0];
                            leftProceeded++;
                            break;
                        case 4:
                            leftCurrent += "\\" + pathTerms[1];
                            leftProceeded++;
                            break;
                        default:
                            if (direction % 2 == 1)
                            {
                                rightProceeded++;
                                rightCurrent = pathTerms[pathTerms.Length - rightProceeded] + "\\" + rightCurrent;
                            }
                            else
                            {
                                leftCurrent += "\\" + pathTerms[leftProceeded];
                                leftProceeded++;
                            }
                            break;
                    }
                } while ((leftCurrent + "\\Е\\" + rightCurrent).Length <= maxLength && leftProceeded + rightProceeded <= pathTerms.Length);

                if ((leftProceeded == 0 && rightProceeded == 1 && pathTerms.Length > 1 && rightCurrent.Length <= maxLength)
                    || (leftProceeded == 1 && rightProceeded == 1 && pathTerms.Length > 2 && rightCurrent.Length <= maxLength))
                    last = rightCurrent;
                else if (last == "\\Е\\")
                    last = "Е";
            }

            result = last.Length < fileFullName.Length ? last : fileFullName;
            switch (alignment)
            {
                case TextAlignment.None:
                    break;
                case TextAlignment.Left:
                    for (int i = last.Length; i < maxLength; i++)
                        result += padding;
                    break;
                case TextAlignment.Center:
                    for (int i = last.Length; i < maxLength; i++)
                        if (i % 2 == 0)
                            result += padding;
                        else
                            result = padding + result;
                    break;
                case TextAlignment.Right:
                    for (int i = last.Length; i < maxLength; i++)
                        result = padding + result;
                    break;
            }

            return result;
        }
    }
}