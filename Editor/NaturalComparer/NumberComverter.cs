
using System.Text;
using System.Collections;
using System.Globalization;

namespace NaturalSortOrder
{
	internal enum CharTypes : uint
	{
		/*!> なし */
		kNone = 0x0,
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
	}
	internal interface INumberComverter
	{
		/*!> エラーが発生したかどうかを取得します */
		bool IsError{ get; }
		/*!> 数字から変換した数値を取得します */
		long Value{ get; }
		/*!> 数値全体の文字数を取得します */
		int Length{ get; }

		/**
		 * \brief 数字を追加します
		 * \param number [in] 数字を表す
		 * \return 追加結果。以下の値が返ります。
		 * \retval true 数字として成立する場合に返ります。
		 * \retval false 数字として成立しない場合に返ります。
		 */
		bool AddChar( char number);
	}
	/**
	 * \brief アラビア数字を数値へ変換
	 */
	internal class NumberConverter : INumberComverter
	{
		public NumberConverter( char number)
		{
			if( number >= '0' && number <= '9')
			{
				numberZero = '0';
				numberNine = '9';
			}
			else
			{
				numberZero = '０';
				numberNine = '９';
			}

			length = 1;
			value = number - numberZero;
			isComma = false;
		}

		/*!> エラーが発生したかどうかを取得します */
		public bool IsError
		{
			get{ return false; }
		}
		/*!> 数字から変換した数値を取得します */
		public long Value
		{
			get{ return value; }
		}
		/*!> 数値全体の文字数を取得します */
		public int Length
		{
			get{ return length; }
		}
		/**
		 * \brief 数字を追加します
		 * \param number [in] 数字を表す
		 * \return 追加結果。以下の値が返ります。
		 * \retval true 数字として成立する場合に返ります。
		 * \retval false 数字として成立しない場合に返ります。
		 */
		public bool AddChar( char number)
		{
			/* 1文字目の数字と同種のアラビア数字かどうか */
			if (number >= numberZero && number <= numberNine)
			{
				if( isComma != false)
				{
					++commaLength;
					
					if( commaLength > 3)
					{
						length = numberCount;
						return false;
					}
				}
				else
				{
					++numberCount;
				}
				value = value * 10 + (number - numberZero);
			}
			/* 3桁区切りのカンマかどうか */
			else if( numberZero - number == 4)
			{
				if( isComma == false && numberCount > 3)
				{
					return false;
				}
				commaLength = 0;
			}
			/* アラビア数字以外の文字が見つかったら終了 */
			else
			{
				return false;
			}

			length++;
			return true;
		}
		
		/*!> 数値全体の長さを表す */
		int length;
		/*!> アラビア数字の 0 を表す */
		char numberZero;
		/*!> アラビア数字の 9 を表す */
		char numberNine;
		/*!> 変換結果の数値を表す */
		long value;
		/*!> カンマ区切りの数値かどうかを表す */
		bool isComma;
		/*!> 先頭から連続したアラビア数字の文字数を表す */
		int numberCount;
		/*!> カンマ区切り以降のアラビア数字の文字数を表す */
		int commaLength;
	}
	/// <summary>英字表現のローマ数字を数値へ変換する機能を提供します。</summary>
	/// <remarks>大文字と小文字、ASCIIと日本語の混在は許しません。</remarks>
	class RomanNumberConverter : INumberComverter
	{
		public RomanNumberConverter( char alpha)
		{
			if( alpha >= 'A' && alpha <= 'Z')
			{
				alphaA = 'A';
			}
			else if( alpha >= 'a' && alpha <= 'z')
			{
				alphaA = 'a';
			}
			else if( alpha >= 'Ａ' && alpha <= 'Ｚ')
			{
				alphaA = 'Ａ';
			}
			else if( alpha >= 'ａ' && alpha <= 'ｚ')
			{
				alphaA = 'ａ';
			}

			length = 1;
			number = Parse( alpha);
			max = number;
		}
		/*!> エラーが発生したかどうかを取得します */
		public bool IsError
		{
			get{ return isError; }
		}
		/*!> 数字から変換した数値を取得します */
		public long Value
		{
			get{ return value + number; }
		}
		/*!> 数値全体の文字数を取得します */
		public int Length
		{
			get{ return length; }
		}
		/**
		 * \brief 数字を追加します
		 * \param roman [in] 数字を表す
		 * \return 追加結果。以下の値が返ります。
		 * \retval true 数字として成立する場合に返ります。
		 * \retval false 数字として成立しない場合に返ります。
		 */
		public bool AddChar( char roman)
		{
			long value = Parse( roman);

			/* ローマ数字以外の文字が見つかったら終了 */
			if( value == 0)
			{
				isError = IsAlpha( roman);
				return false;
			}
			/* IV IX などの減算則表記 */
			else if( value > max)
			{
				long mag = value / max;
				if( mag == 5 || mag == 10)
				{
					value += value - number;
					number = 0;
					max = max / 2;
				}
				else
				{
					isError = IsAlpha( roman);
					return false;
				}
			}
			/* VI XI など加算則表記 */
			else if( value < max)
			{
				value += number;
				number = value;
				max = value;
			}
			/* II XX など同じ数字の繰り返し */
			else
			{
				number += value;
			}
			length++;
			return true;
		}
		/**
		 * \brief ローマ数字を数値へ変換します
		 * \param alpha [in] ローマ数字
		 * \return ローマ数字が示す値が返ります。
		 */
		protected long Parse( char alpha)
		{
			switch( alpha - alphaA)
			{
				case 08: return 1;		/* I */
				case 21: return 5;		/* V */
				case 23: return 10; 	/* X */
				case 11: return 50; 	/* L */
				case 02: return 100;	/* C */
				case 03: return 500;	/* D */
				case 12: return 1000;	/* M */
			}
			return 0;
		}
		/**
		 * \brief 指定の文字が英字かどうかを判定する
		 * \param alpha [in] 検査対象の文字
		 * \return 判定結果。以下の値が返ります。
		 * \retval true 指定の文字が英字だった場合に返ります。
		 * \retval false 指定の文字が英字ではない場合に返ります。
		 */
		protected bool IsAlpha( char alpha)
		{
			return ((alpha >= 'A' && alpha <= 'Z') || (alpha >= 'a' && alpha <= 'z') || (alpha >= 'Ａ' && alpha <= 'Ｚ') || (alpha >= 'ａ' && alpha <= 'ｚ'));
		}

		/*!>数値全体の長さを表す int */
		int length;
		/*!>アルファベットの A を表す char */
		char alphaA;
		/*!>未確定の数字を表す long */
		long number;
		/*!>変換結果の数値を表す long */
		long value;
		/*!>現在単位を表す long */
		long max;
		/*!>エラーが発生したかどうかを表す bool */
		bool isError;
	}
	/// <summary>全角ローマ数字を数値へ変換する機能を提供します。</summary>
	class JpRomanNumberConverter : INumberComverter
	{
		public JpRomanNumberConverter( char roman)
		{
			length = 1;

			/* 全角ローマ数字(Ⅰ～XII,L,C,D,M) */
			if( roman >= 0x2160 && roman <= 0x216F)
			{
				romanOne = (char)0x2160;
			}
			/* 全角ローマ数字(ⅰ～xii,l,c,d,m) */
			else if( roman >= 0x2170 && roman <= 0x217F)
			{
				romanOne = (char)0x2170;
			}

			long value = Parse( roman);
			if( value == 0)
			{
				value = roman - romanOne + 1;
				isMultiChar = false;
			}
			else
			{
				number = value;
				max = number;
				isMultiChar = true;
			}
		}
		/*!> エラーが発生したかどうかを取得します */
		public bool IsError
		{
			get{ return false; }
		}
		/*!> 数字から変換した数値を取得します */
		public long Value
		{
			get{ return value + number; }
		}

		/*!> 数値全体の文字数を取得します */
		public int Length
		{
			get{ return length; }
		}
		/**
		 * \brief 数字を追加します
		 * \param roman [in] 数字を表す
		 * \return 追加結果。以下の値が返ります。
		 * \retval true 数字として成立する場合に返ります。
		 * \retval false 数字として成立しない場合に返ります。
		 */
		public bool AddChar( char roman)
		{
			if( isMultiChar == false)
			{
				return false;
			}

			long value = Parse( roman);

			/* ローマ数字以外の文字が見つかったら終了 */
			if( value == 0)
			{
				return false;
			}
			/* IV IX などの減算則表記 */
			else if( value > max)
			{
				long mag = value / max;
				if( mag == 5 || mag == 10)
				{
					value += value - number;
					number = 0;
					max = max / 2;
				}
				else
				{
					return false;
				}
			}
			/* VI XI など加算則表記 */
			else if( value < max)
			{
				value += number;
				number = value;
				max = value;
			}
			/* II XX など同じ数字の繰り返し */
			else
			{
				number += value;
			}

			length++;
			return true;
		}
		/**
		 * \brief ローマ数字を数値へ変換します
		 * \param alpha [in] ローマ数字
		 * \return ローマ数字が示す値が返ります。
		 */
		protected long Parse( char roman)
		{
			switch( roman - romanOne)
			{
				case 0x0: return 1; 	/* I */
				case 0x4: return 5; 	/* V */
				case 0x9: return 10;	/* X */
				case 0xC: return 50;	/* L */
				case 0xD: return 100;	/* C */
				case 0xE: return 500;	/* D */
				case 0xF: return 1000;	/* M */
			}
			return 0;
		}
		
		/*!> 数値全体の長さを表す int */
		int length;
		/*!> 2文字以上の組み合わせが可能かどうかを表す bool */
		bool isMultiChar;
		/*!> ローマ数字の 1 を表す char */
		char romanOne;
		/*!> 未確定の数字を表す long */
		long number;
		/*!> 変換結果の数値を表す long */
		long value;
		/*!> 現在単位を表す long */
		long max;
	}
	/// <summary>丸数字を数値へ変換する機能を提供します。</summary>
	class CircleNumberConverter : INumberComverter
	{
		public CircleNumberConverter( char number)
		{
			/* ①～⑳ */
			if( number >= 0x2460 && number <= 0x2473)
			{
				this.number = number - 0x2460 + 1;
			}
			/* (1)～(12) */
			else if( number >= 0x2474 && number <= 0x2487)
			{
				this.number = number - 0x2474 + 1;
			}
			/* 1.～20. */
			else if( number >= 0x2488 && number <= 0x249B)
			{
				this.number = number - 0x2488 + 1;
			}
			/* 丸付き21～35 */
			else if( number >= 0x3251 && number <= 0x325F)
			{
				this.number = number - 0x3251 + 21;
			}
			/* {一}～{十} */
			else if( number >= 0x3220 && number <= 0x3229)
			{
				this.number = number - 0x3220 + 1;
			}
			/* 丸付き一～十 */
			else if( number >= 0x3280 && number <= 0x3289)
			{
				this.number = number - 0x3280 + 1;
			}
		}
		/*!> エラーが発生したかどうかを取得します */
		public bool IsError
		{
			get{ return false; }
		}
		/*!> 数字から変換した数値を取得します */
		public long Value
		{
			get{ return number; }
		}
		/*!> 数値全体の文字数を取得します */
		public int Length
		{
			get{ return 1; }
		}
		/**
		 * \brief 数字を追加します
		 * \param number [in] 数字を表す
		 * \return 追加結果。以下の値が返ります。
		 * \retval true 数字として成立する場合に返ります。
		 * \retval false 数字として成立しない場合に返ります。
		 */
		public bool AddChar( char number)
		{
			return false;
		}
		
		/*!> 現在の数字を表す */
		long number;
	}
	/// <summary>漢数字を数値へ変換する機能を提供します。</summary>
	class KanjiNumberConverter : INumberComverter
	{
		/// <summary>インスタンスを初期化します。</summary>
		/// <param name="number">１文字目の数字を表す char。</param>
		public KanjiNumberConverter( char number)
		{
			length = 1;
			
			long temp = Parse( number);
			if( temp < 10)
			{
				this.number = temp;
				unit1 = 9999;
				unit2 = 99999999999999999;
			}
			else
			{
				value1 = unit1 = temp;
			}
		}
		/*!> エラーが発生したかどうかを取得します */
		public bool IsError
		{
			get{ return false; }
		}
		/*!> 数字から変換した数値を取得します */
		public long Value
		{
			get{ return value2 + value1 + number; }
		}
		/*!> 数値全体の文字数を取得します */
		public int Length
		{
			get{ return length; }
		}
		/**
		 * \brief 数字を追加します
		 * \param kanji [in] 数字を表す
		 * \return 追加結果。以下の値が返ります。
		 * \retval true 数字として成立する場合に返ります。
		 * \retval false 数字として成立しない場合に返ります。
		 */
		public bool AddChar( char kanji)
		{
			long value = Parse( kanji);

			/* 2文字目の内容で位取り記数法かどうかを決定する */
			if( length == 1)
			{
				isNumeral = (number + value1 < 10 && value < 10);
				if( isNumeral)
				{
					value2 = number;
					number = 0;
				}
			}
			if( value < 0)
			{
				return false;
			}
			if( isNumeral != false)
			{
				if( value > 10)
				{
					return false;
				}

				value2 = value2 * 10 + value;
				length++;
				return true;
			}
			if( value < 10)
			{
				/* 9以下の漢数字が連続したらエラー */
				if( number > 0)
				{
					return false;
				}
				number = value;
			}
			else if( value <= 1000)
			{
				/* 前方より大きな単位が出現したらエラー */
				if( unit1 <= value)
				{
					return false;
				}
				value1 += number * value;
				number = 0;
				unit1 = value;
			}
			else
			{
				/* 前方より大きな単位が出現したらエラー */
				if( unit2 <= value)
				{
					return false;
				}
				value2 += (value1 + number) * value;
				value1 = number = 0;
				unit1 = 9999;
				unit2 = value;
			}
			length++;
			return true;
		}
		/**
		 * \brief 漢数字を数値へ変換します
		 * \param alpha [in] 漢数字
		 * \return 漢数字が示す値が返ります。
		 */
		protected long Parse( char kanji)
		{
			switch( kanji)
			{
				case '〇': return 0;
				case '一': return 1;
				case '二': return 2;
				case '三': return 3;
				case '四': return 4;
				case '五': return 5;
				case '六': return 6;
				case '七': return 7;
				case '八': return 8;
				case '九': return 9;
				case '十': return 10;
				case '百': return 100;
				case '千': return 1000;
				case '万': return 10000;
				case '億': return 100000000;
				case '兆': return 1000000000000;
				case '京': return 10000000000000000;
				case '零': return 0;
				case '壱': return 1;
				case '弐': return 2;
				case '参': return 3;
				case '拾': return 10;
			}
			return -1;
		}

		/*!> 数値全体の長さを表す int */
		int length;
		/*!> 位取り記数法かどうかを表す bool */
		bool isNumeral;
		/*!> 直前の数字を表す long */
		long number;
		/*!> 1万未満の数値を表す long */
		long value1;
		/*!> 変換結果の数値を表す long */
		long value2;
		/*!> 1万未満の現在単位を表す long */
		long unit1;
		/*!> 数値全体の現在単位を表す long */
		long unit2;
	}
}
