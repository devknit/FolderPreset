
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
		 * \param source [in] 数字を表す
		 * \return 追加結果。以下の値が返ります。
		 * \retval true 数字として成立する場合に返ります。
		 * \retval false 数字として成立しない場合に返ります。
		 */
		bool AddChar( char value);
	}
	/**
	 * \brief アラビア数字を数値へ変換
	 */
	internal class NumberConverter : INumberComverter
	{
		public NumberConverter( char source)
		{
			if( source >= '0' && source <= '9')
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
			value = source - numberZero;
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
		public bool AddChar( char source)
		{
			/* 1文字目の数字と同種のアラビア数字かどうか */
			if (source >= numberZero && source <= numberNine)
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
				value = value * 10 + (source - numberZero);
			}
			/* 3桁区切りのカンマかどうか */
			else if( numberZero - source == 4)
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
	/**
	 * \brief 英字表現のローマ数字を数値へ変換する\n
	 *        大文字と小文字、ASCIIと日本語の混在は許容しない
	 */
	class RomanNumberConverter : INumberComverter
	{
		public RomanNumberConverter( char source)
		{
			if( source >= 'A' && source <= 'Z')
			{
				alphaA = 'A';
			}
			else if( source >= 'a' && source <= 'z')
			{
				alphaA = 'a';
			}
			else if( source >= 'Ａ' && source <= 'Ｚ')
			{
				alphaA = 'Ａ';
			}
			else if( source >= 'ａ' && source <= 'ｚ')
			{
				alphaA = 'ａ';
			}

			length = 1;
			number = Parse( source);
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
		 * \param source [in] 数字を表す
		 * \return 追加結果。以下の値が返ります。
		 * \retval true 数字として成立する場合に返ります。
		 * \retval false 数字として成立しない場合に返ります。
		 */
		public bool AddChar( char source)
		{
			long parseValue = Parse( source);

			/* ローマ数字以外の文字が見つかったら終了 */
			if( parseValue == 0)
			{
				isError = IsAlpha( source);
				return false;
			}
			/* IV IX などの減算則表記 */
			else if( parseValue > max)
			{
				long mag = parseValue / max;
				if( mag == 5 || mag == 10)
				{
					value += parseValue - number;
					number = 0;
					max = max / 2;
				}
				else
				{
					isError = IsAlpha( source);
					return false;
				}
			}
			/* VI XI など加算則表記 */
			else if( parseValue < max)
			{
				value += number;
				number = parseValue;
				max = parseValue;
			}
			/* II XX など同じ数字の繰り返し */
			else
			{
				number += parseValue;
			}
			length++;
			return true;
		}
		/**
		 * \brief ローマ数字を数値へ変換します
		 * \param source [in] ローマ数字
		 * \return ローマ数字が示す値が返ります。
		 */
		protected long Parse( char source)
		{
			switch( source - alphaA)
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
		 * \param source [in] 検査対象の文字
		 * \return 判定結果。以下の値が返ります。
		 * \retval true 指定の文字が英字だった場合に返ります。
		 * \retval false 指定の文字が英字ではない場合に返ります。
		 */
		protected bool IsAlpha( char source)
		{
			return ((source >= 'A' && source <= 'Z') || (source >= 'a' && source <= 'z') || (source >= 'Ａ' && source <= 'Ｚ') || (source >= 'ａ' && source <= 'ｚ'));
		}

		/*!>数値全体の長さを表す */
		int length;
		/*!>アルファベットの A を表す */
		char alphaA;
		/*!>未確定の数字を表す */
		long number;
		/*!>変換結果の数値を表す */
		long value;
		/*!>現在単位を表す */
		long max;
		/*!>エラーが発生したかどうかを表す */
		bool isError;
	}
	/**
	 * \brief 全角ローマ数字を数値へ変換する
	 */
	class JpRomanNumberConverter : INumberComverter
	{
		public JpRomanNumberConverter( char source)
		{
			length = 1;

			/* 全角ローマ数字(Ⅰ～XII,L,C,D,M) */
			if( source >= 0x2160 && source <= 0x216F)
			{
				romanOne = (char)0x2160;
			}
			/* 全角ローマ数字(ⅰ～xii,l,c,d,m) */
			else if( source >= 0x2170 && source <= 0x217F)
			{
				romanOne = (char)0x2170;
			}

			long parseValue = Parse( source);
			if( parseValue == 0)
			{
				value = source - romanOne + 1;
				isMultiChar = false;
			}
			else
			{
				number = parseValue;
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
		 * \param source [in] 数字を表す
		 * \return 追加結果。以下の値が返ります。
		 * \retval true 数字として成立する場合に返ります。
		 * \retval false 数字として成立しない場合に返ります。
		 */
		public bool AddChar( char source)
		{
			if( isMultiChar == false)
			{
				return false;
			}

			long parseValue = Parse( source);

			/* ローマ数字以外の文字が見つかったら終了 */
			if( parseValue == 0)
			{
				return false;
			}
			/* IV IX などの減算則表記 */
			else if( parseValue > max)
			{
				long mag = parseValue / max;
				if( mag == 5 || mag == 10)
				{
					value += parseValue - number;
					number = 0;
					max = max / 2;
				}
				else
				{
					return false;
				}
			}
			/* VI XI など加算則表記 */
			else if( parseValue < max)
			{
				value += number;
				number = parseValue;
				max = parseValue;
			}
			/* II XX など同じ数字の繰り返し */
			else
			{
				number += parseValue;
			}

			length++;
			return true;
		}
		/**
		 * \brief ローマ数字を数値へ変換します
		 * \param alpha [in] ローマ数字
		 * \return ローマ数字が示す値が返ります。
		 */
		protected long Parse( char source)
		{
			switch( source - romanOne)
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
	/**
	 * \brief 丸数字を数値へ変換する
	 */
	class CircleNumberConverter : INumberComverter
	{
		public CircleNumberConverter( char source)
		{
			/* ①～⑳ */
			if( source >= 0x2460 && source <= 0x2473)
			{
				number = source - 0x2460 + 1;
			}
			/* (1)～(12) */
			else if( source >= 0x2474 && source <= 0x2487)
			{
				number = source - 0x2474 + 1;
			}
			/* 1.～20. */
			else if( source >= 0x2488 && source <= 0x249B)
			{
				number = source - 0x2488 + 1;
			}
			/* 丸付き21～35 */
			else if( source >= 0x3251 && source <= 0x325F)
			{
				number = source - 0x3251 + 21;
			}
			/* {一}～{十} */
			else if( source >= 0x3220 && source <= 0x3229)
			{
				number = source - 0x3220 + 1;
			}
			/* 丸付き一～十 */
			else if( source >= 0x3280 && source <= 0x3289)
			{
				number = source - 0x3280 + 1;
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
		 * \param source [in] 数字を表す
		 * \return 追加結果。以下の値が返ります。
		 * \retval true 数字として成立する場合に返ります。
		 * \retval false 数字として成立しない場合に返ります。
		 */
		public bool AddChar( char source)
		{
			return false;
		}
		
		/*!> 現在の数字を表す */
		long number;
	}
	/**
	 * \brief 漢数字を数値へ変換する
	 */
	class KanjiNumberConverter : INumberComverter
	{
		public KanjiNumberConverter( char source)
		{
			length = 1;
			
			long temp = Parse( source);
			if( temp < 10)
			{
				number = temp;
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
		 * \param source [in] 数字を表す
		 * \return 追加結果。以下の値が返ります。
		 * \retval true 数字として成立する場合に返ります。
		 * \retval false 数字として成立しない場合に返ります。
		 */
		public bool AddChar( char source)
		{
			long parseValue = Parse( source);

			/* 2文字目の内容で位取り記数法かどうかを決定する */
			if( length == 1)
			{
				isNumeral = (number + value1 < 10 && parseValue < 10);
				if( isNumeral != false)
				{
					value2 = number;
					number = 0;
				}
			}
			if( parseValue < 0)
			{
				return false;
			}
			if( isNumeral != false)
			{
				if( parseValue > 10)
				{
					return false;
				}

				value2 = value2 * 10 + parseValue;
				length++;
				return true;
			}
			if( parseValue < 10)
			{
				/* 9以下の漢数字が連続したらエラー */
				if( number > 0)
				{
					return false;
				}
				number = parseValue;
			}
			else if( parseValue <= 1000)
			{
				/* 前方より大きな単位が出現したらエラー */
				if( unit1 <= parseValue)
				{
					return false;
				}
				value1 += number * parseValue;
				number = 0;
				unit1 = parseValue;
			}
			else
			{
				/* 前方より大きな単位が出現したらエラー */
				if( unit2 <= parseValue)
				{
					return false;
				}
				value2 += (value1 + number) * parseValue;
				value1 = number = 0;
				unit1 = 9999;
				unit2 = parseValue;
			}
			length++;
			return true;
		}
		/**
		 * \brief 漢数字を数値へ変換します
		 * \param source [in] 漢数字
		 * \return 漢数字が示す値が返ります。
		 */
		protected long Parse( char source)
		{
			switch( source)
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
