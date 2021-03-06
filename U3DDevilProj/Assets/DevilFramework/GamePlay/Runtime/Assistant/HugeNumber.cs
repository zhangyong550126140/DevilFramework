﻿using Devil.Utility;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Devil.GamePlay.Assistant
{
    public struct HugeNumber
    {
        public const string SCIENTIFIC_PATTERN = @"^(\+|\-)?\d+(.)?\d*(E(\+|\-)?\d+)?$";

        byte[] mDatas;
        bool mNegative;
        int mOffset;

        public static HugeNumber Zero
        {
            get
            {
                HugeNumber num;
                num.mOffset = 0;
                num.mNegative = false;
                num.mDatas = null;
                return num;
            }
        }

        public HugeNumber One
        {
            get
            {
                HugeNumber num = Zero;
                num.SetABit(0);
                return num;
            }
        }

        public HugeNumber(long num)
        {
            mNegative = num < 0;
            mOffset = 0;
            mDatas = null;
            SetValue(num);
        }

        public HugeNumber(string num)
        {
            mOffset = 0;
            mNegative = false;
            mDatas = null;
            SetValue(num);
        }

        public HugeNumber(HugeNumber num)
        {
            mNegative = num.mNegative;
            if (num.mDatas != null)
            {
                int len = num.mDatas.Length;
                mDatas = new byte[len];
                System.Array.Copy(num.mDatas, mDatas, len);
            }
            else
            {
                mDatas = null;
            }
            mOffset = num.mOffset;
        }

        public void SetValue(HugeNumber num)
        {
            if (num.IsZero)
            {
                Reset();
            }
            else
            {
                mNegative = num.mNegative;
                int size = num.mOffset + 1;
                Malloc(size);
                System.Array.Copy(num.mDatas, mDatas, size);
                mOffset = num.mOffset;
            }
        }

        public void SetABit(int bit, byte value = 1)
        {
            Malloc(bit + 1);
            for (int i = 0; i <= mOffset; i++)
            {
                mDatas[i] = 0;
            }
            mDatas[bit] = value;
            mOffset = bit;
        }

        public byte GetBit(int bit)
        {
            if (mDatas != null && bit < mDatas.Length)
                return mDatas[bit];
            else
                return 0;
        }

        public int Bits { get { return mOffset; } }

        public void SetValue(long value)
        {
            if (value == 0)
            {
                Reset();
                return;
            }
            SetValue(value.ToString());
        }

        public void SetValue(string num, int pow = 0)
        {
            if (string.IsNullOrEmpty(num))
                Reset();
            else if (Regex.IsMatch(num, SCIENTIFIC_PATTERN))
            {
                int mid = num.IndexOf('E');
                int mov = pow;
                if (mid == -1)
                {
                    mov = pow;
                    mid = num.Length;
                }
                else
                {
                    mov += int.Parse(num.Substring(mid + 1));
                }
                mNegative = num[0] == '-';
                int off = mNegative || num[0] == '+' ? 1 : 0;
                int p = 0;
                mOffset = 0;
                for (int i = mid - 1; i >= off; i--)
                {
                    char c = num[i];
                    if (c >= '0' && c <= '9')
                    {
                        Malloc(p + 1);
                        if (c != '0')
                            mOffset = p;
                        mDatas[p++] = (byte)(c - '0');
                    }
                    else if (c == '.')
                    {
                        mOffset = p;
                        mov -= p;
                    }
                }
                MoveLeft(mov);
            }
            else
            {
                throw new System.Exception(string.Format("Can't parse {0} as a number.", num));
            }
        }

        // 获取第一位不为0的位置
        int GetOffset(int offset)
        {
            for (int i = offset; i >= 0; i--)
            {
                if (mDatas[i] != 0)
                    return i;
            }
            return 0;
        }

        public void Reset()
        {
            mNegative = false;
            if (mDatas != null)
            {
                for (int i = mOffset; i >= 0; i--)
                {
                    mDatas[i] = 0;
                }
            }
            mOffset = 0;
        }

        public void Reverse()
        {
            mNegative = !mNegative;
        }

        public void Abs()
        {
            mNegative = false;
        }

        void Malloc(int newsize)
        {
            if (mDatas == null)
            {
                mDatas = new byte[Mathf.Max(32, newsize)];
                mOffset = 0;
            }
            else if (newsize > mDatas.Length)
            {
                int len = mDatas.Length;
                byte[] datas = new byte[Mathf.Max(newsize, mDatas.Length * 2)];
                System.Array.Copy(mDatas, datas, mDatas.Length);
                mDatas = datas;
            }
        }

        public bool IsZero { get { return mOffset == 0 && (mDatas == null || mDatas[0] == 0); } }

        public void MoveLeft(int bits)
        {
            if (bits == 0 || IsZero)
            {
                return;
            }
            else if (bits > 0)
            {
                mOffset += bits;
                Malloc(mOffset + 1);
                for (int i = mOffset; i >= bits; i--)
                {
                    mDatas[i] = mDatas[i - bits];
                }
                for (int i = bits - 1; i >= 0; i--)
                {
                    mDatas[i] = 0;
                }
            }
            else
            {
                int p = mOffset + bits;
                mOffset = Mathf.Max(0, p);
                for (int i = 0; i <= p; i++)
                {
                    mDatas[i] = mDatas[i - bits];
                }
                int reset = Mathf.Max(-1, p);
                for (int i = p - bits; i > reset; i--)
                {
                    mDatas[i] = 0;
                }
            }
        }

        public void MoveRight(int bits)
        {
            MoveLeft(-bits);
        }

        public void Add(HugeNumber num)
        {
            if (num.IsZero)
                return;
            int size = Mathf.Max(num.mOffset, mOffset);
            Malloc(size + 1);
            if (num.mNegative ^ mNegative)
            {
                int cmp = CompareAbsTo(num);
                if (cmp == 0)
                {
                    Reset();
                }
                else
                {
                    // 借位
                    int borrow = 0;
                    int v;
                    if (cmp < 0)
                    {
                        for (int i = 0; i <= size; i++)
                        {
                            v = num.mDatas[i] - borrow - mDatas[i];
                            if (v < 0)
                            {
                                v += 10;
                                borrow = 1;
                            }
                            else
                            {
                                borrow = 0;
                            }
                            mDatas[i] = (byte)v;
                        }
                        mOffset = GetOffset(size);
                        mNegative = !mNegative;
                    }
                    else
                    {
                        for (int i = 0; i <= size; i++)
                        {
                            v = mDatas[i] - borrow - num.mDatas[i];
                            if (v < 0)
                            {
                                v += 10;
                                borrow = 1;
                            }
                            else
                            {
                                borrow = 0;
                            }
                            mDatas[i] = (byte)v;
                        }
                        mOffset = GetOffset(size);
                    }
                }
            }
            else
            {
                int v;
                int stepin = 0;
                for (int i = 0; i <= size; i++)
                {
                    v = mDatas[i] + num.mDatas[i] + stepin;
                    if (v >= 10)
                    {
                        v -= 10;
                        stepin = 1;
                    }
                    else
                    {
                        stepin = 0;
                    }
                    mDatas[i] = (byte)v;
                }
                if (stepin != 0)
                {
                    Malloc(size + 2);
                    mDatas[size + 1] = (byte)(mDatas[size + 1] + stepin);
                }
                mOffset = GetOffset(mDatas.Length - 1);
            }
        }

        public void Substract(HugeNumber num)
        {
            num.Reverse();
            Add(num);
            num.Reverse();
        }

        public void Multiply(HugeNumber num)
        {
            if (IsZero)
                return;
            if (num.IsZero)
            {
                Reset();
                return;
            }
            mNegative = mNegative ^ num.mNegative;
            HugeNumber temp = new HugeNumber(this);
            Reset();
            int a, b;
            int stepin = 0;
            int v;
            int p;
            for (int i = 0; i <= num.mOffset; i++)
            {
                if (num.mDatas[i] == 0)
                {
                    continue;
                }
                for (int j = 0; j <= temp.mOffset; j++)
                {
                    p = i + j;
                    Malloc(p + 1);
                    a = temp.mDatas[j];
                    b = num.mDatas[i];
                    v = a * b + stepin + mDatas[p];
                    mDatas[p] = (byte)(v % 10);
                    stepin = v / 10;
                }
                if (stepin > 0)
                {
                    p = i + temp.mOffset + 1;
                    Malloc(p + 1);
                    mDatas[p] = (byte)stepin;
                    stepin = 0;
                }
            }
            mOffset = GetOffset(mDatas.Length - 1);
        }

        public void Divide(HugeNumber num)
        {
            if (num.IsZero)
                throw new System.Exception(string.Format("{0} divided by 0 or null was not allowed.", this));
            if (IsZero)
                return;
            if (CompareAbsTo(num) < 0)
            {
                Reset();
                return;
            }
            mNegative = mNegative ^ num.mNegative;
            HugeNumber tmp = new HugeNumber(this);
            tmp.Abs();
            HugeNumber div = new HugeNumber(num);
            div.Abs();
            Reset();
            HugeNumber sum = Zero;
            while (tmp.CompareTo(div) >= 0)
            {
                int mov = tmp.mOffset - div.mOffset;
                div.MoveLeft(mov);
                if (tmp.CompareTo(div) < 0)
                {
                    mov--;
                    div.MoveRight(1);
                }
                tmp.Substract(div);
                if (tmp.CompareTo(Zero) >= 0)
                {
                    sum.SetABit(mov);
                    Add(sum);
                    div.MoveRight(mov);
                }
                else
                {
                    break;
                }
            }
        }

        /// <summary>
        /// 计算指数
        /// </summary>
        /// <param name="times"></param>
        /// <param name="precision">记录计算过程中的精度（即原数乘了10^precision倍）</param>
        public void Pow(int times, int precision = 0)
        {
            if (times < 0)
            {
                Reset();
            }
            else if (times == 0)
            {
                SetABit(precision);
            }
            else
            {
                HugeNumber cache = new HugeNumber(this);
                HugeNumber temp = One;
                temp.MoveLeft(precision);
                SetABit(precision);
                int p = 0;
                for (int i = 0; i < 32; i++)
                {
                    if ((times & (1 << i)) == 0)
                        continue;
                    while (p <= i)
                    {
                        temp.Multiply(cache);
                        temp.MoveRight(precision);
                        cache.SetValue(temp);
                        p++;
                    }
                    Multiply(temp);
                    MoveRight(precision);
                }
            }
        }

        public long ToLong()
        {
            if (IsZero)
                return 0;
            long v = 0;
            if (mDatas != null)
            {
                for (int i = mOffset; i >= 0; i--)
                {
                    v *= 10;
                    v += mDatas[i];
                }
            }
            return mNegative ? -v : v;
        }

        public override string ToString()
        {
            if (mOffset >= 9)
            {
                return ToScientificNum();
            }
            else
            {
                return ToThousandsString();
            }
        }

        /// <summary>
        /// 转换为科学计数法
        /// </summary>
        /// <param name="max">最高保留位数</param>
        /// <param name="fixZero">清理末尾的 0</param>
        /// <returns></returns>
        public string ToScientificNum(int max = 9, bool fixZero = true)
        {
            if (IsZero)
                return "0";
            StringBuilder builder = StringUtil.GetBuilder();
            if (mNegative)
                builder.Append('-');
            int n = 0;
            int dotp = 1; // .
            for (int i = mOffset; i >= 0; i--)
            {
                builder.Append((char)(mDatas[i] + '0'));
                if (i == mOffset)
                {
                    builder.Append('.');
                    dotp = builder.Length;
                }
                n++;
                if (n >= max)
                    break;
            }
            if (fixZero)
            {
                for (int i = builder.Length - 1; i > dotp; i--)
                {
                    if (builder[i] == '0')
                        builder.Remove(i, 1);
                    else
                        break;
                }
            }
            builder.Append("E+").Append(mOffset - 1);
            return StringUtil.ReleaseBuilder(builder);
        }

        public string ToThousandsString()
        {
            if (IsZero)
                return "0";
            StringBuilder builder = StringUtil.GetBuilder();
            if (mNegative)
                builder.Append('-');
            for (int i = mOffset; i >= 0; i--)
            {
                builder.Append((char)(mDatas[i] + '0'));
                if (i != 0 && i % 3 == 0)
                    builder.Append(',');
            }
            return StringUtil.ReleaseBuilder(builder);
        }

        public string ToRawString()
        {
            if (IsZero)
                return "0";
            var buf = StringUtil.GetBuilder();
            if (mNegative)
                buf.Append('-');
            for (int i = mOffset; i >= 0; i--)
            {
                buf.Append((char)(mDatas[i] + '0'));
            }
            return StringUtil.ReleaseBuilder(buf);
        }

        public int CompareAbsTo(HugeNumber b)
        {
            if (b.IsZero)
                return IsZero ? 0 : 1;
            else if (IsZero)
                return -1;
            if (mOffset != b.mOffset)
                return mOffset > b.mOffset ? 1 : -1;
            byte ca, cb;
            for (int i = mOffset; i >= 0; i--)
            {
                ca = mDatas[i];
                cb = b.mDatas[i];
                if (ca < cb)
                    return -1;
                else if (ca > cb)
                    return 1;
            }
            return 0;
        }

        public int CompareTo(HugeNumber b)
        {
            if (b.IsZero)
                return IsZero ? 0 : (mNegative ? -1 : 1);
            if (mNegative ^ b.mNegative)
                return mNegative ? -1 : 1;
            return mNegative ? b.CompareAbsTo(this) : this.CompareAbsTo(b);
        }

        public static HugeNumber operator +(HugeNumber a, HugeNumber b)
        {
            HugeNumber num = new HugeNumber(a);
            num.Add(b);
            return num;
        }

        public static HugeNumber operator -(HugeNumber a, HugeNumber b)
        {
            HugeNumber num = new HugeNumber(a);
            num.Substract(b);
            return num;
        }

        public static HugeNumber operator *(HugeNumber a, HugeNumber b)
        {
            HugeNumber num = new HugeNumber(a);
            num.Multiply(b);
            return num;
        }

        public static HugeNumber operator /(HugeNumber a, HugeNumber b)
        {
            HugeNumber num = new HugeNumber(a);
            num.Divide(b);
            return num;
        }

        public static HugeNumber operator -(HugeNumber a)
        {
            HugeNumber num = new HugeNumber(a);
            num.Reverse();
            return num;
        }

        public static bool operator >(HugeNumber a, HugeNumber b)
        {
            return a.CompareTo(b) > 0;
        }

        public static bool operator <(HugeNumber a, HugeNumber b)
        {
            return a.CompareTo(b) < 0;
        }

        public static bool operator >=(HugeNumber a, HugeNumber b)
        {
            return a.CompareTo(b) >= 0;
        }

        public static bool operator <=(HugeNumber a, HugeNumber b)
        {
            return a.CompareTo(b) <= 0;
        }

        public static bool operator ==(HugeNumber a, HugeNumber b)
        {
            return a.CompareTo(b) == 0;
        }

        public static bool operator !=(HugeNumber a, HugeNumber b)
        {
            return a.CompareTo(b) != 0;
        }

        public override bool Equals(object obj)
        {
            return obj is HugeNumber ? CompareTo((HugeNumber)obj) == 0 : false;
        }

        public override int GetHashCode()
        {
            return StringUtil.ToHash(ToString());
        }

    }
}