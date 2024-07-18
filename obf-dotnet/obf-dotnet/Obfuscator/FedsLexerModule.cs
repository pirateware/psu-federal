using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace obf_dotnet.Obfuscator
{
    public static class FedsLexerModule
    {
		public static List<int> takenRandomNumbers = new List<int>();
		public static int GetRandomNumber(int min, int max)
		{
			int ret = 0;
			ret = _rnd.Next(min, max);


			if (takenRandomNumbers.IndexOf(ret) != -1)
			{
				ret = GetRandomNumber(min, max);
			}
			takenRandomNumbers.Add(ret);
			return ret;

		}
		public static string[] StripLocals(string script)
		{
			List<string> locals = new List<string>();
			Regex reg = new Regex("local(.*?)[;=]");
			foreach (Match m in reg.Matches(script))
			{
				string loc = m.Groups[1].Value.Replace(" ", "");
				if (!locals.Contains(loc))
					locals.Add(loc);

				if (!m.Value.Contains(";"))
					script = script.Replace($"local{m.Groups[1].Value}", loc);
				else
					script = script.Replace($"local{m.Groups[1].Value};", "");
			}
			string localnames = "";
			foreach (string l in locals)
				localnames += " local " + l + ";\n";

			return new string[] { localnames, script };

		}
		public class RemovedStrings
		{
			public string OriginalScript { get; set; }
			public Dictionary<string, string> HiddenStrings { get; set; }
		}
		public static RemovedStrings RemoveStrings(string input)
		{

			const string encRegex = @"(['""])?(?(1)((?:[^\\]|\\.)*?)\1|\[(=*)\[(.*?)\]\3\])";
			string _src = input;

			Regex r = new Regex(encRegex, RegexOptions.Singleline | RegexOptions.Compiled);

			int indDiff = 0;
			var matches = r.Matches(_src);
			Dictionary<string, string> strs = new Dictionary<string, string>();

			foreach (Match m in matches)
			{
				string before = _src.Substring(0, m.Index + indDiff);
				string after = _src.Substring(m.Index + indDiff + m.Length);


				string nStr = "";

				string placeholder = FedsLexerModule.GetRandom(30);
				strs.Add("\"" + placeholder + "\"", m.Groups[0].Value);
				nStr = before + "\"" + placeholder + "\""; //  // escaped, yikes
				nStr += after;
				indDiff += nStr.Length - _src.Length;
				_src = nStr;
			}



			return new RemovedStrings()
			{
				OriginalScript = _src,
				HiddenStrings = strs

			};
		}


		public static string HideStrings(string input)
		{
			const string stringRegex = @"(['""])?(?(1)((?:[^\\]|\\.)*?)\1|\[(=*)\[(.*?)\]\3\])";
			string _src = input;

			Regex r = new Regex(stringRegex, RegexOptions.Singleline | RegexOptions.Compiled);

			int indDiff = 0;
			var matches = r.Matches(_src);


			foreach (Match m in matches)
			{
				string before = _src.Substring(0, m.Index + indDiff);
				string after = _src.Substring(m.Index + indDiff + m.Length);
				string captured = m.Groups[2].Value + m.Groups[4].Value;

				captured = m.Groups[2].Value == "" ? captured : UnescapeLuaString(captured);
				string _ogstr = captured; // original str
				string nStr = "";
				string seccap = "";


				for (int i = 0; i < captured.Length; i++)
				{

					seccap += GetEscapedLuaUnicode((int)captured[i]);

				}
				captured = seccap;
				nStr = before + "('" + captured + "')"; // escaped, yikes
				nStr += after;
				indDiff += nStr.Length - _src.Length;
				_src = nStr;
			}



			return _src;
		}
		public static string UnescapeLuaString(string str)
		{
			List<char> bytes = new List<char>();

			int i = 0;
			while (i < str.Length)
			{
				char cur = str[i++];
				if (cur == '\\')
				{
					char next = str[i++];

					switch (next)
					{
						case 'a':
							bytes.Add('\a');
							break;

						case 'b':
							bytes.Add('\b');
							break;

						case 'f':
							bytes.Add('\f');
							break;

						case 'n':
							bytes.Add('\n');
							break;

						case 'r':
							bytes.Add('\r');
							break;

						case 't':
							bytes.Add('\t');
							break;

						case 'v':
							bytes.Add('\v');
							break;

						default:
							{
								if (!char.IsDigit(next))
									bytes.Add(next);
								else // \001, \55h, etc
								{
									string s = next.ToString();
									for (int j = 0; j < 2; j++, i++)
									{
										if (i == str.Length)
											break;

										char n = str[i];
										if (char.IsDigit(n))
											s = s + n;
										else
											break;
									}

									bytes.Add((char)int.Parse(s));
								}

								break;
							}
					}
				}
				else
					bytes.Add(cur);
			}

			return new string(bytes.ToArray());
		} // from ib
		public static string GetEscapedLuaUnicode(int unival)
		{
			if (unival < 128) { return "\\" + unival; }

			List<string> charbytes = new List<string>();
			for (int p = 0; p < 5; p++) { charbytes.Add(""); }

			int[][] bytemarkers = { new int[] { }, new int[] { 0, 0x7ff, 192 }, new int[] { 0, 0xffff, 224 }, new int[] { 0, 0x1fffff, 240 } };

			for (int i = 1; i < bytemarkers.Length; i++)
			{

				if (unival <= bytemarkers[i][1])
				{
					for (int x = i + 1; x >= 2; x--)
					{
						int mod = unival % 64;
						unival = (unival - mod) / 64;
						charbytes[x] = "\\" + (128 + mod);
					}
					charbytes[1] = "\\" + (bytemarkers[i][2] + unival);
					break;
				}

			}




			return charbytes.Aggregate((i, j) => i + j);

		} // unicode support shit, from moonsec
		public static Random _rnd = new Random();
		public static List<string> takenRandoms = new List<string>();
		public static string GetRandom(int len = 8, bool isAlphanumeric = true)
		{
			string ret = "";
			string ldb = "abcdefghijklmnoprstuvyzqwxABCDEFGHJKLMOPRITSUVYZQWX_";

			for (int i = 0; i < len; i++)
			{
				ret += isAlphanumeric ? (ldb[_rnd.Next(0, ldb.Length)].ToString()) : "\\" + _rnd.Next(0, 256);

			}


			if (takenRandoms.IndexOf(ret) != -1)
			{
				ret = GetRandom(len, isAlphanumeric);
			}
			takenRandoms.Add(ret);
			return ret;

		}

		public static char[] EndLiterals = {
				(char)39, (char)9, ' ', '"', '(', ')', ']', ';', (char)10, (char)13, '}', '=',
			};
		public static char[] Spaces =  {
			(char)9, ' ', (char)10, (char)13
		};
		public static bool isEndLiteral(char ch)
		{

			return Array.IndexOf(EndLiterals, ch) != -1;

		}
		public static bool isSpace(char ch)
		{

			return Array.IndexOf(Spaces, ch) != -1;
		}

		public static int GetKeywordIndex(string script, string keyword)
		{

			int idx = -1;
			for (int i = 0; i < EndLiterals.Length; i++)
			{
				idx = script.IndexOf(EndLiterals[i] + keyword);
				if (idx != -1)
				{
					for (int k = 0; k < EndLiterals.Length; k++)
					{

						idx = script.IndexOf(EndLiterals[i] + keyword + EndLiterals[k]);
						if (idx != -1)
						{
							idx++; // skips the first end literal 
							return idx;
						}

					}


				}
			}

			return -1;

		}

		public static string[] HandleBreaks(string input, string rtoken)
		{

			int KeywordIndex = GetKeywordIndex(input, "break");
			if (KeywordIndex == -1) { return new string[] { input, "0" }; }

			string mainscript = "\n---BREAK START---\n" + input + "\n---BREAKEND----\n";
			string newscript = "";
			string randomfuncname = rtoken;

			bool allDone = false;
			int Idx = 0;

			while (!allDone)
			{
				string part = mainscript.Substring(Idx);

				int index = GetKeywordIndex(part, "break");
				if (index == -1)
				{
					newscript += " " + part;
					allDone = true;
					break;
				}

				string before = part.Substring(0, index);
				string middle = " \n--BREAK\n break_" + randomfuncname + "()";

				newscript += before + middle;
				Idx += index + 5;


			}

			return new string[] { newscript, "1" };
		}
		public static string HandleContinues(string input)
		{

			int KeywordIndex = GetKeywordIndex(input, "continue");
			if (KeywordIndex == -1) { return input; }

			string mainscript = "\n---CONTINUE LOOP START---\n" + input + "\n---CONTINUE LOOP END----\n";
			string newscript = "";

			bool allDone = false;
			int Idx = 0;

			while (!allDone)
			{
				string part = mainscript.Substring(Idx);

				int index = GetKeywordIndex(part, "continue");
				if (index == -1)
				{
					newscript += " " + part;
					allDone = true;
					break;
				}

				string before = part.Substring(0, index);
				string middle = " \n--CONTINUE\n break";

				newscript += before + middle;
				Idx += index + 8;


			}

			return " repeat\n " + newscript + " \nuntil true\n ";

		}

		public static string FixScript(string script, bool recursedCall = false, int recurses = 0)
		{ // dont call with recursedcall true
			if (!script.Contains("continue")) { return script; }
            string mainscript = "";
			FedsLexerModule.RemovedStrings removedStrings = new RemovedStrings() { };
			if (recursedCall)
            {
				mainscript = script;
            } else
            {
				 removedStrings = FedsLexerModule.RemoveStrings(script);
				mainscript = removedStrings.OriginalScript;
            }
			//string mainscript = !recursedCall ? HideStrings(script) : script;
			if (!mainscript.Contains("continue")) { return script; }
			if (GetKeywordIndex(mainscript, "continue") == -1) { return script; }
			mainscript = "\n---------Continue Detected------------\n" + mainscript + "\n------------------------\n";
			string newscript = "";

			bool allDone = false;
			int Idx = 0;

			while (!allDone)
			{
				int depthLevel = 0; // depth of the line in do-end blocks
				int Top = 0;
				int Bottom = 0;
				int Limiter = 0;
				string part = mainscript.Substring(Idx);

				int index = GetKeywordIndex(part, "continue");
				if (index == -1)
				{
					newscript += " " + part;
					allDone = true;
				}
				if (index != -1)
				{ // gotcha



					// backwards searcher
					for (int i = index - 1; i > 0; i--)
					{
						string a = part[i].ToString();
						// function 
						if (a == "n" && part.Substring(i - 7, 8) == "function" && isEndLiteral(part[i - 8]) && isEndLiteral(part[i + 1]))
						{
							depthLevel++;
							//	System.Windows.Forms.MessageBox.Show("FUNCTION");
							// then
						}
						else if (a == "n" && part.Substring(i - 3, 4) == "then" && isEndLiteral(part[i - 4]) && isEndLiteral(part[i + 1]))
						{
							depthLevel++;
							//	System.Windows.Forms.MessageBox.Show("THEN");

						}


						// elseif
						if (a == "f" && part.Substring(i - 5, 6) == "elseif" && isEndLiteral(part[i - 6]) && isEndLiteral(part[i + 1]))
						{
							depthLevel--;
							//	System.Windows.Forms.MessageBox.Show("ELSEIF");
						}
						// end
						if (a == "d" && part.Substring(i - 2, 3) == "end" && isEndLiteral(part[i - 3]) && isEndLiteral(part[i + 1]))
						{
							depthLevel--;
							//	System.Windows.Forms.MessageBox.Show("END");

						}

						if (depthLevel > Limiter)
						{
							Limiter = depthLevel;
							//	System.Windows.Forms.MessageBox.Show(Limiter + " e ");
						}


						// do
						if (a == "o" && part.Substring(i - 1, 2) == "do" && isEndLiteral(part[i - 2]) && isEndLiteral(part[i + 1]))
						{
							//System.Windows.Forms.MessageBox.Show("DO");

							if (depthLevel == Limiter)
							{
								// reached top level
								//	System.Windows.Forms.MessageBox.Show(depthLevel + ";;" + Limiter);
								Top = i + 1;
								break;
							}
							depthLevel++;
							//	System.Windows.Forms.MessageBox.Show("INC: " + depthLevel);
						}


					}
					depthLevel = Limiter;
					//System.Windows.Forms.MessageBox.Show(depthLevel + " f ");
					// forwards searcher
					for (int i = index + 8; i < part.Length; i++)
					{
						string a = part[i].ToString();
						// function 
						if (a == "n" && part.Substring(i - 7, 8) == "function" && isEndLiteral(part[i - 8]) && isEndLiteral(part[i + 1]))
						{
							depthLevel++;

							// then
						}
						else if (a == "n" && part.Substring(i - 3, 4) == "then" && isEndLiteral(part[i - 4]) && isEndLiteral(part[i + 1]))
						{
							depthLevel++;
						}

						// do
						if (a == "o" && part.Substring(i - 1, 2) == "do" && isEndLiteral(part[i - 2]) && isEndLiteral(part[i + 1]))
						{
							depthLevel++;
						}

						// elseif
						if (a == "f" && part.Substring(i - 5, 6) == "elseif" && isEndLiteral(part[i - 6]) && isEndLiteral(part[i + 1]))
						{
							depthLevel--;
						}
						// end
						if (a == "d" && part.Substring(i - 2, 3) == "end" && isEndLiteral(part[i - 3]) && isEndLiteral(part[i + 1]))
						{
							depthLevel--;
							if (depthLevel == -1)
							{
								// reached bottom level
								Bottom = i - 2;
								break;
							}
						}

					}

					Idx += Bottom;
					// System.Windows.Forms.MessageBox.Show(Bottom + " / " + Top);
					if (Bottom == 0 && Top == 0)
					{
						return mainscript; // invalid keyword usage >> probably uses "continue" as a name (lmao) - but still pass it
					}
					string mod = part.Substring(Top, Bottom - Top);

					string rtoken = GetRandom(12);
					string[] e = HandleBreaks(mod, rtoken);

					mod = HandleContinues(e[0]);

					if (e[1] == "1")
					{ // checks if "break"s are used
						mod = "local shouldBreak_" + rtoken + " = false; local function break_" + rtoken + "() shouldBreak_" + rtoken + " = true; end\n " + mod + " if (shouldBreak_" + rtoken + ") then break end ";
					}


					//System.Windows.Forms.MessageBox.Show(mod);
					newscript += " " + part.Substring(0, Top) + " " + mod + " ";

					/*++:
					then, do, function

					--:
					end, elseif

					**:
					do
					*/








				}


			}

			recurses++;
			if (recurses < 7)
			{ // prevent stackoverflows and shit
				if (recursedCall)
				{
					return FixScript(newscript, true, recurses);
				} else
                {
					string s = FixScript(newscript, true, recurses);
					for (int e = 0; e < removedStrings.HiddenStrings.Keys.Count; e++)
					{
						s = s.Replace(removedStrings.HiddenStrings.ElementAt(e).Key, removedStrings.HiddenStrings.ElementAt(e).Value);
					}
					return s;
				}
			}
			else
			{
				return newscript;
			}
		}
    }
}