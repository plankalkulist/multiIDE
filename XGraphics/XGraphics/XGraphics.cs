using System;
using System.Drawing;

namespace XSpace
{
    public static class XGraphics
    {
        public const int xgGold = 54527;
        public enum StdHues : int
        {
            hueRed = 0,
            hueGold = 50,
            hueYellow = 60,
            hueGreen = 120,
            hueCyan = 180,
            hueBlue = 240,
            hueViolet = 300
        }

        /*Public PX0 As Single, PY0 As Single
        '
        Const RtoG As Single = 180 / 3.14159265358979
        Dim CurX As Single, CurY As Single
        Dim MaxRD As Integer

        Private Sub ErrExitSub(Optional Report As Variant)
        If IsMissing(Report) Then
            If ReportArgumentalErrors Then
                Beep
                Exit Sub
            End If
        Else
            Select Case VarType(Report)
                Case vbBoolean
                    If Report Then Beep
                Case vbString
                    Beep
                    MsgBox Report, vbCritical, "ArgumentalSubCellingError"
                Case Else
                    Beep
                    Beep
            End Select
        End If

        End Sub
        */

        /*Sub DrawColorCircle(GraphBox As Control, Optional RStart As Integer, Optional RLength As Integer)
        'обработка аргументов
        If GraphBox Is Nothing Then ErrExitSub: Exit Sub
        If RLength = 0 Then RLength = MaxRD - RStart
        '
        Dim X1 As Single, Y1 As Single                      'координаты
        Dim gR As Single, rd As Integer                     'счЄтчики
        Dim S As Single, DrSt As Single
        PX0 = GraphBox.Width / 2
        PY0 = GraphBox.Height / 2
        If MaxRD = 0 Then MaxRD = 1490
        '
        DrSt = 0.6

        For rd = RStart To RStart + RLength Step 9
            S = rd / MaxRD* 100
            For gR = 0 To 360 Step DrSt
                GCoord PX0, PY0, rd, gR + 90, X1, Y1
                GraphBox.PSet(X1, Y1), HSL(gR, S, 100)
            Next gR
            DoEvents
        Next rd

        For rd = MaxRD To MaxRD + 4
            S = 100
            For gR = 0 To 360 Step DrSt
                GCoord PX0, PY0, rd, gR + 90, X1, Y1
                GraphBox.PSet(X1, Y1), HSL(gR, S, 100)
            Next gR
            DoEvents
        Next rd

        End Sub
        */

        /* Sub GetHSL(Color As int, Optional OutH As Single, Optional OutS As Single, Optional OutL As Single)
        Dim A As Integer
        Dim H As Single, S As Single, L As Single
        Dim R As Byte, G As Byte, B As Byte
        Dim min As Single, max As Single

        GetRGB Color, R, G, B

        If R >= G And R >= B Then max = R
        If G >= R And G >= B Then max = G
        If B >= R And B >= G Then max = B
        '
        If R <= G And R <= B Then min = R
        If G <= R And G <= B Then min = G
        If B <= R And B <= G Then min = B

        'OutS = Int(min / max * 100 * 2 + 0.5) / 2
        'OutV = Int(max / 255 * 100 * 2 + 0.5) / 2

        OutS = min / max* 100
        OutL = max / 255 * 100

        End Sub
        */

        public static void GetHSV(System.Drawing.Color color, out double outH, out double outS, out double outV)
        {
            double R, G, B;
            double min, max;

            R = color.R / 255.0;
            G = color.G / 255.0;
            B = color.B / 255.0;

            max = R;
            if (G > max) max = G;
            if (B > max) max = B;
            min = B;
            if (G < min) min = G;
            if (R < min) min = R;

            outH = 0;
            if (max == min)
                outH = 0;
            else if (max == R && G >= B)
                outH = 60 * (G - B) / (max - min);
            else if (max == R && G < B)
                outH = 360 - 60 * (B - G) / (max - min);
            else if (max == G)
                outH = 120 + 60 * (B - R) / (max - min);
            else if (max == B)
                outH = 240 + 60 * (R - G) / (max - min);

            if (max == 0)
                outS = 100 * 0;
            else
                outS = 100 * (1 - (min / max));

            outV = 100 * max;
        }

        /*Sub GetRGB(Color As int, Optional OutR As Integer, Optional OutG As Integer, Optional OutB As Integer)
        OutR = CByte(Val("&H" + Right(String(6 - Len(Hex(Color)), "0") + Hex(Color), 2)))
        OutG = CByte(Val("&H" + Mid(String(6 - Len(Hex(Color)), "0") + Hex(Color), 3, 2)))
        OutB = CByte(Val("&H" + Left(String(6 - Len(Hex(Color)), "0") + Hex(Color), 2)))

        End Sub
        */

        /* 'Hue - цветовой тон(0-359.5), Saturation - насыщенность(0-100.0), Lightness - €ркость(0-100.0)
        '
        Function HSL(ByVal Hue As StdHues, Saturation As Single, Lightness As Single) As int
        'обработка аргументов
        If Hue > 360 Then Hue = Hue Mod 360
        If Hue< 0 Then Hue = Hue Mod 360 + 360
        If Saturation > 100 Then Saturation = 100
        If Saturation < 0 Then Saturation = 0
        If Lightness > 100 Then Lightness = 100
        If Lightness < 0 Then Lightness = 0
        Hue = Int(Hue * 2 + 0.5) / 2
        Saturation = Int(Saturation * 2 + 0.5) / 2
        Lightness = Int(Lightness * 2 + 0.5) / 2
        '
        Dim A As Integer
        Dim R As Byte, G As Byte, B As Byte
        Dim H As Single, S As Single, L As Single

        H = (Hue \ 60) * 60
        S = Saturation

        Select Case H
            Case hueRed, 360
                H = Hue - H
                R = 255
                G = CByte(255 * (100 - S) / 100 + 255 * S / 100 * (H / 60))
                B = 255 * (100 - S) / 100
            Case hueYellow
                H = Hue - H
                R = 255 - CByte(255 * S / 100 * (H / 60))
                G = 255
                B = 255 * (100 - S) / 100
            Case hueGreen
                H = Hue - H
                R = 255 * (100 - S) / 100
                G = 255
                B = CByte(255 * (100 - S) / 100 + 255 * S / 100 * (H / 60))
            Case hueCyan
                H = Hue - H
                R = 255 * (100 - S) / 100
                G = 255 - CByte(255 * S / 100 * (H / 60))
                B = 255
            Case hueBlue
                H = Hue - H
                R = CByte(255 * (100 - S) / 100 + 255 * S / 100 * (H / 60))
                G = 255 * (100 - S) / 100
                B = 255
            Case hueViolet
                H = Hue - H
                R = 255
                G = 255 * (100 - S) / 100
                B = 255 - CByte(255 * S / 100 * (H / 60))
        End Select

        R = R* (Lightness / 100)
        G = G* (Lightness / 100)
        B = B* (Lightness / 100)

        HSL = RGB(R, G, B)

        End Function
                    */

        /// <summary>
        /// ‘ункци€ вычисл€ет значение цвета типа Color из параметров модели HSV.
        /// </summary>
        /// <param name="hue">цветовой тон (0-359,5).</param>
        /// <param name="saturation">насыщенность (0-100,0).</param>
        /// <param name="value">€ркость (0-100,0).</param>
        /// <param name="opacity">непрозрачность (0-255) - то же самое, что и alpha-канал. ѕо умолчанию равна 255.</param>
        /// <returns>цвет типа Color</returns>
        public static Color HSV(double hue, double saturation, double value, int opacity = 255)
        {
            //обработка аргументов
            if (hue >= 360)
                hue = hue % 360;
            if (hue < 0)
                hue = hue % 360 + 360;
            if (saturation > 100)
                saturation = 100;
            if (saturation < 0)
                saturation = 0;
            if (value > 100)
                value = 100;
            if (value < 0)
                value = 0;
            hue = Math.Truncate(hue * 2 + 0.5) / 2;
            saturation = Math.Truncate(saturation * 2 + 0.5) / 2;
            value = Math.Truncate(value * 2 + 0.5) / 2;
            //
            double R = 0;
            double G = 0;
            double B = 0;
            StdHues iH = 0;
            double H = 0;
            double S = 0;

            iH = (StdHues)((int)Math.Truncate(hue / 60) * 60);
            S = saturation;

            switch (iH)
            {
                case StdHues.hueRed:
                    H = hue - (double)iH;
                    R = 255;
                    G = 255 * (100 - S) / 100 + 255 * S / 100 * (H / 60);
                    B = 255 * (100 - S) / 100;
                    break;
                case StdHues.hueYellow:
                    H = hue - (double)iH;
                    R = 255 - (255 * S / 100 * (H / 60));
                    G = 255;
                    B = 255 * (100 - S) / 100;
                    break;
                case StdHues.hueGreen:
                    H = hue - (double)iH;
                    R = 255 * (100 - S) / 100;
                    G = 255;
                    B = 255 * (100 - S) / 100 + 255 * S / 100 * (H / 60);
                    break;
                case StdHues.hueCyan:
                    H = hue - (double)iH;
                    R = 255 * (100 - S) / 100;
                    G = 255 - (255 * S / 100 * (H / 60));
                    B = 255;
                    break;
                case StdHues.hueBlue:
                    H = hue - (double)iH;
                    R = 255 * (100 - S) / 100 + 255 * S / 100 * (H / 60);
                    G = 255 * (100 - S) / 100;
                    B = 255;
                    break;
                case StdHues.hueViolet:
                    H = hue - (double)iH;
                    R = 255;
                    G = 255 * (100 - S) / 100;
                    B = 255 - (255 * S / 100 * (H / 60));
                    break;
            }

            R = R * (value / 100);
            G = G * (value / 100);
            B = B * (value / 100);

            return Color.FromArgb(opacity, (byte)R, (byte)G, (byte)B);
        }

        /* Function Gradient(FromColor As int, ToColor As int, Optional Percent As Single) As int
        'обработка аргументов
        If IsMissing(Percent) Or Not IsNumeric(Percent) Then Percent = 50
        '
        Dim A As Double, B As Double
        Dim fR As Integer, fG As Integer, fB As Integer
        Dim tR As Integer, tG As Integer, tB As Integer
        Dim gR As Single, gG As Single, gB As Single

        GetRGB FromColor, fR, fG, fB
        GetRGB ToColor, tR, tG, tB

        A = Percent / 100
        B = tR - fR
        gR = fR + B * A
        gG = fG + (tG - fG) * Percent / 100
        gB = fB + (tB - fB) * Percent / 100

        Gradient = RGB(CByte(gR), CByte(gG), CByte(gB))

        End Function

        Private Sub GCoord(x0 As Single, y0 As Single, R As Integer, _
        Gradus As Single, OutPut_X As Single, Output_Y As Single)
        Dim A As Single

        A = (Gradus - 90) / RtoG

        OutPut_X = x0 + Cos(A) * R
        Output_Y = y0 + Sin(A) * R
        End Sub*/
    }
}