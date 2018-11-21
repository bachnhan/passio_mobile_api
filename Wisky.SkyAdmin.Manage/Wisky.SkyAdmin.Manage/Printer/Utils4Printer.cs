using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Wisky.SkyAdmin.Manage.Printer
{
    public class Utils4Printer
    {
        // Case normal. Exp: 1, 2, 34, 68,...
        public static Dictionary<string, string> normalDict = new Dictionary<string, string>()
        {
            {Properties.Resources.ZERO_1_VN, Properties.Resources.ZERO},
            {Properties.Resources.ONE_1_VN, Properties.Resources.ONE},
            {Properties.Resources.TWO_VN, Properties.Resources.TWO},
            {Properties.Resources.THREE_VN , Properties.Resources.THREE},
            {Properties.Resources.FOUR_VN , Properties.Resources.FOUR},
            {Properties.Resources.FIVE_1_VN , Properties.Resources.FIVE},
            {Properties.Resources.SIX_VN, Properties.Resources.SIX},
            {Properties.Resources.SEVEN_VN , Properties.Resources.SEVEN},
            {Properties.Resources.EIGHT_VN , Properties.Resources.EIGHT},
            {Properties.Resources.NINE_VN , Properties.Resources.NINE}
        };

        // Case abnormal. Exp: 21, 75,...
        public static Dictionary<string, string> abnormalDict = new Dictionary<string, string>()
        {
            {Properties.Resources.ONE_2_VN, Properties.Resources.ONE},
            {Properties.Resources.TWO_VN, Properties.Resources.TWO},
            {Properties.Resources.THREE_VN , Properties.Resources.THREE},
            {Properties.Resources.FOUR_VN , Properties.Resources.FOUR},
            {Properties.Resources.FIVE_2_VN , Properties.Resources.FIVE},
            {Properties.Resources.SIX_VN, Properties.Resources.SIX},
            {Properties.Resources.SEVEN_VN , Properties.Resources.SEVEN},
            {Properties.Resources.EIGHT_VN , Properties.Resources.EIGHT},
            {Properties.Resources.NINE_VN , Properties.Resources.NINE}
        };

        /// <summary>
        /// Convert input number to words
        /// </summary>
        /// <param name="inputNum">The input number.</param>
        /// <returns></returns>
        public string ConvertNum2Words(double inputNum)
        {
            StringBuilder result = new StringBuilder();

            // format string of number
            string strInputNum = inputNum.ToString("N0", CultureInfo.CreateSpecificCulture("de-DE"));
            string[] parts = strInputNum.Split('.');
            int length = parts.Length;

            // loop for each part of 3 number character
            foreach (var part in parts)
            {
                bool flag = true;
                char[] characters = part.ToCharArray();
                switch (characters.Length)
                {
                    // case only 1 character. Exp: 3, 5, 9,...
                    case 1:
                        var unit1 = normalDict.FirstOrDefault(x => x.Value == characters[0].ToString()).Key;
                        result.Append(unit1.ToString() + Properties.Resources.WHITE_SPACE);
                        break;

                    // case 2 character. Exp: 31, 55, 97,... 
                    case 2:
                        if (Properties.Resources.ONE.Equals(characters[0].ToString())) // case 1x. Exp: 13, 15, 18,...
                        {
                            result.Append(Properties.Resources.TENTH_1_VN + Properties.Resources.WHITE_SPACE);
                            if (Properties.Resources.ONE.Equals(characters[1].ToString())) // case 11
                            {
                                result.Append(Properties.Resources.ONE_1_VN + Properties.Resources.WHITE_SPACE);
                            }
                            else // case 1x with x != 1. Exp: 12, 15, 18, 19,...
                            {
                                var unit2 = abnormalDict.FirstOrDefault(x => x.Value == characters[1].ToString()).Key;
                                result.Append(unit2.ToString() + Properties.Resources.WHITE_SPACE);
                            }
                        }
                        else // case not 1x. Exp: 2x, 3x, 5x, 7x,...
                        {
                            var tenth2 = normalDict.FirstOrDefault(x => x.Value == characters[0].ToString()).Key;
                            result.Append(tenth2.ToString() + Properties.Resources.WHITE_SPACE + Properties.Resources.TENTH_2_VN + Properties.Resources.WHITE_SPACE);

                            // case not 1x with x != 0. Exp: 25, 34, 82
                            if (!Properties.Resources.ZERO.Equals(characters[1].ToString()))
                            {
                                var unit2 = abnormalDict.FirstOrDefault(x => x.Value == characters[1].ToString()).Key;
                                result.Append(unit2.ToString() + Properties.Resources.WHITE_SPACE);
                            }
                        }

                        break;

                    // case 3 character. Exp: 312, 556, 978,...
                    case 3:

                        // case 000
                        if (Properties.Resources.ZERO.Equals(characters[0].ToString()) && Properties.Resources.ZERO.Equals(characters[1].ToString()) && Properties.Resources.ZERO.Equals(characters[2].ToString()))
                        {
                            flag = false;
                        }
                        else // case not 000. Exp: 012, 416, 891,...
                        {
                            var hundreds = normalDict.FirstOrDefault(x => x.Value == characters[0].ToString()).Key;
                            result.Append(hundreds.ToString() + Properties.Resources.WHITE_SPACE + Properties.Resources.HUNDREDS_VN + Properties.Resources.WHITE_SPACE);

                            // case not x00 with x != 0
                            if (!Properties.Resources.ZERO.Equals(characters[1].ToString()) || !Properties.Resources.ZERO.Equals(characters[2].ToString()))
                            {
                                // case x0y. Exp: 201, 305, 506, 704,...
                                if (Properties.Resources.ZERO.Equals(characters[1].ToString()))
                                {
                                    result.Append(Properties.Resources.ZERO_2_VN + Properties.Resources.WHITE_SPACE);

                                    var unit3 = normalDict.FirstOrDefault(x => x.Value == characters[2].ToString()).Key;
                                    result.Append(unit3.ToString() + Properties.Resources.WHITE_SPACE);
                                }
                                else if (Properties.Resources.ONE.Equals(characters[1].ToString())) // case x1y. Exp: 113, 915, 618,...
                                {
                                    result.Append(Properties.Resources.TENTH_1_VN + Properties.Resources.WHITE_SPACE);

                                    if (Properties.Resources.ONE.Equals(characters[2].ToString())) // case x11
                                    {
                                        result.Append(Properties.Resources.ONE_1_VN + Properties.Resources.WHITE_SPACE);
                                    }
                                    else // case x1y with y != 1. Exp: x12, x15, x18, x19,...
                                    {
                                        // case not x1y with y != 0 && y != 1. Exp: 254, 347, 829
                                        if (!Properties.Resources.ZERO.Equals(characters[2].ToString()))
                                        {
                                            var unit3 = abnormalDict.FirstOrDefault(x => x.Value == characters[2].ToString()).Key;
                                            result.Append(unit3.ToString() + Properties.Resources.WHITE_SPACE);
                                        }

                                    }
                                }
                                else // case not x0y and not x1y. Exp: 243, 975, 628,...
                                {
                                    var tenth3 = normalDict.FirstOrDefault(x => x.Value == characters[1].ToString()).Key;
                                    result.Append(tenth3.ToString() + Properties.Resources.WHITE_SPACE + Properties.Resources.TENTH_2_VN + Properties.Resources.WHITE_SPACE);

                                    // case not x0y and not x1y with y != 0 && y != 1. Exp: 254, 347, 829
                                    if (!Properties.Resources.ZERO.Equals(characters[2].ToString()))
                                    {
                                        var unit3 = abnormalDict.FirstOrDefault(x => x.Value == characters[2].ToString()).Key;
                                        result.Append(unit3.ToString() + Properties.Resources.WHITE_SPACE);
                                    }
                                }
                            }
                        }

                        break;
                }

                if (flag)
                {
                    switch (length)
                    {
                        // case thousands
                        case 2:
                        case 5:
                            result.Append(Properties.Resources.THOUSANDS_VN + Properties.Resources.WHITE_SPACE);
                            break;

                        // case millions
                        case 3:
                        case 6:
                            result.Append(Properties.Resources.MILLIONS_VN + Properties.Resources.WHITE_SPACE);
                            break;

                        // case billions
                        case 4:
                        case 7:
                            result.Append(Properties.Resources.BILLIONS_VN + Properties.Resources.WHITE_SPACE);
                            break;
                    }
                }

                length--;
            }

            return result.ToString();
        }

        /// <summary>
        /// Format number to string with thousands seperators
        /// </summary>
        /// <param name="inputNum"></param>
        /// <returns></returns>
        public string FormatNumber(double inputNum)
        {
            return inputNum.ToString("N0", CultureInfo.CreateSpecificCulture("de-DE"));
        }
    }       
}