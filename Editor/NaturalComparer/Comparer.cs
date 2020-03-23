
using System;
using System.Text;
using System.Collections;
using System.Globalization;

namespace NaturalSortOrder
{
	public enum SortOrder : int
	{
		/*!> なし */
		kNone = 0,
		/*!> 昇順 */
		kAscending = 1,
		/*!> 降順 */
		kDescending = 2,
	}
	public enum ComparerOptions
	{
		/*!> アラビア数字 */
		kNumber = 0x1,
		/*!> ASCIIローマ数字 */
		kRomanNumber = 0x2,
		/*!> 日本語ローマ数字 */
		kJpRomanNumber = 0x4,
		/*!> 日本語丸数字 */
		kCircleNumber = 0x8,
		/*!> 日本語漢数字 */
		kKanjiNumber = 0x10,
		/*!> すべての数字 */
		kNumberAll = kNumber | kRomanNumber | kJpRomanNumber | kCircleNumber | kKanjiNumber,

		/*!> 空白文字の存在を無視 */
		kIgnoreSpace = 0x10000,
		/*!> 数字表現の違いを無視 */
		kIgnoreNumber = 0x20000,
		/*!> 全角半角の違いを無視 */
		kIgnoreWide = 0x40000,
		/*!> 大文字小文字の違いを無視 */
		kIgnoreCase = 0x80000,
		/*!> カタカナひらがなの違いを無視 */
		kIgnoreKana = 0x100000,
		/*!> すべての無視条件 */
		kIgnoreAll = kIgnoreSpace | kIgnoreNumber | kIgnoreWide | kIgnoreCase | kIgnoreKana,

		/*!> 既定の比較オプション */
		kDefault = kNumberAll | kIgnoreSpace | kIgnoreNumber | kIgnoreWide | kIgnoreCase,
	}
	public class Comparer : IComparer
	{
		public Comparer() :
			this( SortOrder.kAscending, ComparerOptions.kDefault)
		{
		}
		public Comparer( SortOrder order) :
			this( order, ComparerOptions.kDefault)
		{
		}
		public Comparer( ComparerOptions options) :
			this( SortOrder.kAscending, options)
		{
		}
		public Comparer( SortOrder order, ComparerOptions options)
		{
			this.order = order;
			this.options = options;
		}
		public virtual int Compare( object x, object y)
		{
			return InternalCompare( x as string, y as string);
		}
		protected int InternalCompare( string s1, string s2)
		{
			if( string.IsNullOrEmpty( s1) != false)
			{
				return string.IsNullOrEmpty( s2)? 0 : -1;
			}
			else if( string.IsNullOrEmpty( s2) != false)
			{
				return 1;
			}
			CharTypes filter = (CharTypes)(options & ComparerOptions.kNumberAll);
			CharTypes t1 = CharTypes.kNone;
			CharTypes t2 = CharTypes.kNone;
			char c1 = char.MinValue;
			char c2 = char.MinValue;
			int p1 = 0;
			int p2 = 0;
			
			s1 = ConvertChar( s1);
			s2 = ConvertChar( s2);
			
			while( p1 < s1.Length && p2 < s2.Length)
			{
				t1 = GetCharType( s1[ p1], c1, t1) & filter;
				t2 = GetCharType( s2[ p2], c2, t2) & filter;
				c1 = s1[ p1];
				c2 = s2[ p2];
				
				/* 両方とも何らかの数字の場合 */
				if( (IgnoreNumber != false || (IgnoreNumber == false && t1 == t2)) && t1 != CharTypes.kNone && t2 != CharTypes.kNone)
				{
					int i1 = p1;
					int i2 = p2;
					long v1 = 0;
					long v2 = 0;
					
					bool success = GetNumber( s1, t1, ref i1, out v1) && GetNumber( s2, t2, ref i2, out v2);
					if( success != false)
                    {
                        if( v1 < v2)
                        {
                            return -1;
                        }
                        else if( v1 > v2)
                        {
                            return 1;
                        }
                        p1 = i1;
                        p2 = i2;
                    }
                    else
                    {
                        int diff = CompareChar( s1[ p1], s2[ p2]);
                        if( diff != 0)
                        {
                            return diff;
                        }
                        p1++;
                        p2++;
                    }
				}
				/* いずれかが数字の場合 */
                else if( (t1 != CharTypes.kNone || t2 != CharTypes.kNone) && t1 != CharTypes.kRomanNumber && t2 != CharTypes.kRomanNumber)
                {
                    return (t1 != CharTypes.kNone) ? 1 : -1;
                }
                /* 数字でない場合は文字コードを比較する */
                else
                {
                    int diff = CompareChar( s1[ p1], s2[ p2]);
                    if( diff != 0)
                    {
                        return diff;
                    }
                    p1++;
                    p2++;
                }
			}
			/* 共通部分が一致している場合は、残りの文字列長で大小関係を決める */
            if( p1 >= s1.Length)
            {
                return (p2 >= s2.Length) ? 0 : -1;
            }
            return 1;
		}
		/*!> 空白文字の存在を無視する */
		protected virtual bool IgnoreSpace
		{
			get { return ((options & ComparerOptions.kIgnoreSpace) == ComparerOptions.kIgnoreSpace); }
		}
		/*!> 数字表現の違いを無視する */
		protected virtual bool IgnoreNumber
		{
			get { return ((options & ComparerOptions.kIgnoreNumber) == ComparerOptions.kIgnoreNumber); }
		}
		/*!> 全角半角の違いを無視する */
		protected virtual bool IgnoreWide
		{
			get { return ((options & ComparerOptions.kIgnoreWide) == ComparerOptions.kIgnoreWide); }
		}
		/*!> 大文字小文字の違いを無視する */
		protected virtual bool IgnoreCase
		{
			get { return ((options & ComparerOptions.kIgnoreCase) == ComparerOptions.kIgnoreCase); }
		}
		/*!> カタカナひらがなの違いを無視する */
		protected virtual bool IgnoreKana
		{
			get { return ((options & ComparerOptions.kIgnoreKana) == ComparerOptions.kIgnoreKana); }
		}
		string ConvertChar( string source)
		{
			var buffer = new StringBuilder( source);
			
			if( IgnoreWide != false)
			{
				ConvertHalf( buffer);
			}
			if( IgnoreCase != false)
			{
				ConvertUpperCase( buffer);
			}
			if( IgnoreKana != false)
			{
				ConvertKatakana( buffer);
			}
			return buffer.ToString();
		}
		static void ConvertHalf( StringBuilder source)
		{
			for( int i0 = 0; i0 < source.Length; ++i0)
			{
				if( source[ i0] >= '！' && source[ i0] <= '～')
				{
					source[ i0] = (char)(source[ i0] - '！' + '!');
				}
				else
				{
					switch( source[ i0])
					{
						case '、': source[ i0] = '､'; break;
						case '。': source[ i0] = '｡'; break;
						case '〈': source[ i0] = '<'; break;
						case '〉': source[ i0] = '>'; break;
						case '《': source[ i0] = '<'; break;
						case '》': source[ i0] = '>'; break;
						case '「': source[ i0] = '｢'; break;
						case '」': source[ i0] = '｣'; break;
						case '『': source[ i0] = '｢'; break;
						case '』': source[ i0] = '｣'; break;
						case '【': source[ i0] = '['; break;
						case '】': source[ i0] = ']'; break;
						case '〔': source[ i0] = '['; break;
						case '〕': source[ i0] = ']'; break;
					}
				}
			}
		}
		static void ConvertUpperCase( StringBuilder source)
		{
			for( int i0 = 0; i0 < source.Length; ++i0)
			{
				if( (source[ i0] >= 'a' && source[ i0] <= 'z') || (source[ i0] >= 'ａ' && source[ i0] <= 'ｚ'))
				{
					source[ i0] = char.ToUpper( source[ i0], CultureInfo.InvariantCulture);
				}
			}
		}
		static void ConvertKatakana( StringBuilder source)
		{
			for( int i0 = 0; i0 < source.Length; ++i0)
			{
				if( source[ i0] >= 'ぁ' && source[ i0] <= 'ゞ')
				{
					source[ i0] = (char)(source[ i0] + 'ァ' - 'ぁ');
				}
				else if( source[ i0] >= 'ｦ' && source[ i0] <= 'ﾟ')
				{
					bool replaced = false;

					if( i0 + 1 < source.Length)
					{
						replaced = true;

						switch( source[ i0 + 1])
						{
							case 'ﾞ':
							{
								switch( source[ i0])
								{
									case 'ｶ': source[ i0] = 'ガ'; break;
									case 'ｷ': source[ i0] = 'ギ'; break;
									case 'ｸ': source[ i0] = 'グ'; break;
									case 'ｹ': source[ i0] = 'ゲ'; break;
									case 'ｺ': source[ i0] = 'ゴ'; break;
									case 'ｻ': source[ i0] = 'ザ'; break;
									case 'ｼ': source[ i0] = 'ジ'; break;
									case 'ｽ': source[ i0] = 'ズ'; break;
									case 'ｾ': source[ i0] = 'ゼ'; break;
									case 'ｿ': source[ i0] = 'ゾ'; break;
									case 'ﾀ': source[ i0] = 'ダ'; break;
									case 'ﾁ': source[ i0] = 'ヂ'; break;
									case 'ﾂ': source[ i0] = 'ヅ'; break;
									case 'ﾃ': source[ i0] = 'デ'; break;
									case 'ﾄ': source[ i0] = 'ド'; break;
									case 'ﾊ': source[ i0] = 'バ'; break;
									case 'ﾋ': source[ i0] = 'ビ'; break;
									case 'ﾌ': source[ i0] = 'ブ'; break;
									case 'ﾍ': source[ i0] = 'ベ'; break;
									case 'ﾎ': source[ i0] = 'ボ'; break;
									case 'ｳ': source[ i0] = 'ヴ'; break;
									default: replaced = false; break;
								}
								break;
							}
							case 'ﾟ':
							{
								switch( source[ i0])
								{
									case 'ﾊ': source[ i0] = 'パ'; break;
									case 'ﾋ': source[ i0] = 'ピ'; break;
									case 'ﾌ': source[ i0] = 'プ'; break;
									case 'ﾍ': source[ i0] = 'ペ'; break;
									case 'ﾎ': source[ i0] = 'ポ'; break;
									default: replaced = false; break;
								}
								break;
							}
							default:
							{
								replaced = false;
								break;
							}
						}
						if( replaced != false)
						{
							source.Remove( i0 + 1, 1);
						}
					}
					if( replaced == false)
					{
						switch( source[ i0])
						{
							case 'ｦ': source[ i0] = 'ヲ'; break;
							case 'ｧ': source[ i0] = 'ァ'; break;
							case 'ｨ': source[ i0] = 'ィ'; break;
							case 'ｩ': source[ i0] = 'ゥ'; break;
							case 'ｪ': source[ i0] = 'ェ'; break;
							case 'ｫ': source[ i0] = 'ォ'; break;
							case 'ｬ': source[ i0] = 'ャ'; break;
							case 'ｭ': source[ i0] = 'ュ'; break;
							case 'ｮ': source[ i0] = 'ョ'; break;
							case 'ｯ': source[ i0] = 'ッ'; break;
							case 'ｰ': source[ i0] = 'ー'; break;
							case 'ｱ': source[ i0] = 'ア'; break;
							case 'ｲ': source[ i0] = 'イ'; break;
							case 'ｳ': source[ i0] = 'ウ'; break;
							case 'ｴ': source[ i0] = 'エ'; break;
							case 'ｵ': source[ i0] = 'オ'; break;
							case 'ｶ': source[ i0] = 'カ'; break;
							case 'ｷ': source[ i0] = 'キ'; break;
							case 'ｸ': source[ i0] = 'ク'; break;
							case 'ｹ': source[ i0] = 'ケ'; break;
							case 'ｺ': source[ i0] = 'コ'; break;
							case 'ｻ': source[ i0] = 'サ'; break;
							case 'ｼ': source[ i0] = 'シ'; break;
							case 'ｽ': source[ i0] = 'ス'; break;
							case 'ｾ': source[ i0] = 'セ'; break;
							case 'ｿ': source[ i0] = 'ソ'; break;
							case 'ﾀ': source[ i0] = 'タ'; break;
							case 'ﾁ': source[ i0] = 'チ'; break;
							case 'ﾂ': source[ i0] = 'ツ'; break;
							case 'ﾃ': source[ i0] = 'テ'; break;
							case 'ﾄ': source[ i0] = 'ト'; break;
							case 'ﾅ': source[ i0] = 'ナ'; break;
							case 'ﾆ': source[ i0] = 'ニ'; break;
							case 'ﾇ': source[ i0] = 'ヌ'; break;
							case 'ﾈ': source[ i0] = 'ネ'; break;
							case 'ﾉ': source[ i0] = 'ノ'; break;
							case 'ﾊ': source[ i0] = 'ハ'; break;
							case 'ﾋ': source[ i0] = 'ヒ'; break;
							case 'ﾌ': source[ i0] = 'フ'; break;
							case 'ﾍ': source[ i0] = 'ヘ'; break;
							case 'ﾎ': source[ i0] = 'ホ'; break;
							case 'ﾏ': source[ i0] = 'マ'; break;
							case 'ﾐ': source[ i0] = 'ミ'; break;
							case 'ﾑ': source[ i0] = 'ム'; break;
							case 'ﾒ': source[ i0] = 'メ'; break;
							case 'ﾓ': source[ i0] = 'モ'; break;
							case 'ﾔ': source[ i0] = 'ヤ'; break;
							case 'ﾕ': source[ i0] = 'ユ'; break;
							case 'ﾖ': source[ i0] = 'ヨ'; break;
							case 'ﾗ': source[ i0] = 'ラ'; break;
							case 'ﾘ': source[ i0] = 'リ'; break;
							case 'ﾙ': source[ i0] = 'ル'; break;
							case 'ﾚ': source[ i0] = 'レ'; break;
							case 'ﾛ': source[ i0] = 'ロ'; break;
							case 'ﾜ': source[ i0] = 'ワ'; break;
							case 'ﾝ': source[ i0] = 'ン'; break;
							case 'ﾞ': source[ i0] = '゛'; break;
							case 'ﾟ': source[ i0] = '゜'; break;
						}
					}
				}
			}
		}
		CharTypes GetCharType( char c, char back, CharTypes state)
		{
			/* ASCIIアラビア数字 (0～9) */
			if( c >= '0' && c <= '9')
			{
				return CharTypes.kNumber;
			}
			/* 日本語アラビア数字 (０～９) */
			else if( c >= '０' && c <= '９')
			{
				return CharTypes.kNumber;
			}
			/* 日本語丸数字 (①～⑳) */
			else if( c >= '①' && c <= '⑳')
			{
				return CharTypes.kCircleNumber;
			}
			/* ASCII英大文字 (A～Z) */
			else if( c >= 'A' && c <= 'Z')
			{
				/* ASCIIローマ数字 (I,V,X,L,C,D,M) */
				if( back < 'A' || back > 'Z')
				{
					switch( c)
					{
						case 'I':
						case 'V':
						case 'X':
						case 'L':
						case 'C':
						case 'D':
						case 'M':
						{
							return CharTypes.kRomanNumber;
						}
					}
				}
			}
			/* ASCII英小文字 (a～z) */
			else if( c >= 'a' && c <= 'z')
			{
				/* ASCIIローマ数字 (i,v,x,l,c,d,m) */
				if( (back < 'A' || back > 'Z') && (back < 'a' || back > 'z'))
				{
					switch( c)
					{
						case 'i':
						case 'v':
						case 'x':
						case 'l':
						case 'c':
						case 'd':
						case 'm':
						{
							return CharTypes.kRomanNumber;
						}
					}
				}
			}
			/* 日本語英大文字 (Ａ～Ｚ) */
			else if( c >= 'Ａ' && c <= 'Ｚ')
			{
				/* 日本語ローマ数字 (Ｉ,Ｖ,Ｘ,Ｌ,Ｃ,Ｄ,Ｍ) */
				if( back < 'Ａ' || back > 'Ｚ')
				{
					switch( c)
					{
						case 'Ｉ':
						case 'Ｖ':
						case 'Ｘ':
						case 'Ｌ':
						case 'Ｃ':
						case 'Ｄ':
						case 'Ｍ':
						{
							return CharTypes.kRomanNumber;
						}
					}
				}
			}
			/* 日本語英小文字 (ａ～ｚ) */
			else if( c >= 'ａ' && c <= 'ｚ')
			{
				/* 日本語ローマ数字 (ⅰ,ⅴ,ⅹ,ｌ,ｃ,ｄ,ｍ) */
				if( (back < 'Ａ' || back > 'Ｚ') && (back < 'ａ' || back > 'ｚ'))
				{
					switch( c)
					{
						case 'ⅰ':
						case 'ⅴ':
						case 'ⅹ':
						case 'ｌ':
						case 'ｃ':
						case 'ｄ':
						case 'ｍ':
						{
							return CharTypes.kRomanNumber;
						}
					}
				}
			}
			/* ローマ数字 */
			else if( c >= 0x2160 && c <= 0x217F)
			{
				return CharTypes.kJpRomanNumber;
			}
			else
			{
				/* 日本語漢数字 */
				if( state == CharTypes.kKanjiNumber)
				{
					switch( c)
					{
						case '〇':
						case '一':
						case '二':
						case '三':
						case '四':
						case '五':
						case '六':
						case '七':
						case '八':
						case '九':
						case '十':
						case '百':
						case '千':
						case '万':
						case '億':
						case '兆':
						case '京':
						case '壱':
						case '弐':
						case '参':
						case '拾':
						{
							return CharTypes.kKanjiNumber;
						}
					}
				}
				else
				{
					switch( c)
					{
						case '〇':
						case '一':
						case '二':
						case '三':
						case '四':
						case '五':
						case '六':
						case '七':
						case '八':
						case '九':
						case '十':
						case '百':
						case '千':
						case '壱':
						case '弐':
						case '参':
						{
							return CharTypes.kKanjiNumber;
						}
					}
				}
			}
			return CharTypes.kNone;
		}
		/// <summary>2つの文字コードを比較します。</summary>
        /// <param name="c1">比較する文字を表す char。</param>
        /// <param name="c2">比較する文字を表す char。</param>
        /// <returns>2つの文字コードの大小関係を表す int。</returns>
        int CompareChar( char c1, char c2)
        {
            /* 前中後、上中下の整列を考慮する */
            string list = "上前中下後";
            int p1 = list.IndexOf( c1);
            int p2 = list.IndexOf( c2);

            if( p1 >= 0 && p2 >= 0)
            {
                return p1 - p2;
            }
            return StringComparer.CurrentCulture.Compare( c1.ToString(), c2.ToString());
        }

		static bool GetNumber( string source, CharTypes type, ref int pos, out long value)
		{
			INumberComverter number = null;

			switch( type)
			{
				case CharTypes.kNumber: number = new NumberConverter( source[ pos]); break;
				case CharTypes.kRomanNumber: number = new RomanNumberConverter( source[ pos]); break;
				case CharTypes.kJpRomanNumber: number = new JpRomanNumberConverter( source[ pos]); break;
				case CharTypes.kCircleNumber: number = new CircleNumberConverter( source[ pos]); break;
				case CharTypes.kKanjiNumber: number = new KanjiNumberConverter( source[ pos]); break;
			}
			for( int i0 = pos + 1; i0 < source.Length; ++i0)
			{
				if( number.AddChar( source[ i0]) == false)
				{
					break;
				}
			}
			if( number.IsError == false)
			{
				value = number.Value;
				pos += number.Length;
			}
			else
			{
				value = 0;
			}
			return (number.IsError == false);
		}
		
		SortOrder order;
		ComparerOptions options;
	}
}
