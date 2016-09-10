/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.Globalization;
using System.Collections.Generic;

namespace Apache.NMS.Selector
{
    /// <summary>
    /// A couple of numeric values converted to the type of the largest type.
    /// </summary>
    public class AlignedNumericValues
    {
        private object left;
        public object Left
        {
            get { return left; }
        }

        private object right;
        public object Right
        {
            get { return right; }
        }

        private T type;
        public T TypeEnum
        {
            get { return type; }
        }

        public Type Type
        {
            get { return GetType(type); }
        }

        public AlignedNumericValues(object lvalue, object rvalue)
        {
            if(lvalue == null || rvalue == null)
            {
                return;
            }

            T ltypeEnum = GetTypeEnum(lvalue);
            T rtypeEnum = GetTypeEnum(rvalue);

            type = targetType[(int)ltypeEnum][(int)rtypeEnum];

            left  = (ltypeEnum == type ? lvalue : ConvertValue(lvalue, type));
            right = (rtypeEnum == type ? rvalue : ConvertValue(rvalue, type));
        }

        public enum T
        {
            SByteType  =  0, // Signed 8-bit integer (-128 to 127)
            ByteType   =  1, // Unsigned 8-bit integer (0 to 255)
            CharType   =  2, // Unicode 16-bit character (U+0000 to U+ffff)
            ShortType  =  3, // Signed 16-bit integer (-32 768 to 32 767)
            UShortType =  4, // Unsigned 16-bit integer (0 to 65 535)
            IntType    =  5, // Signed 32-bit integer (-2 147 483 648 to 2 147 483 647)
            UIntType   =  6, // Unsigned 32-bit integer (0 to 4 294 967 295)
            LongType   =  7, // Signed 64-bit integer (-9 223 372 036 854 775 808 to 9 223 372 036 854 775 807)
            ULongType  =  8, // Unsigned 64-bit integer (0 to 18 446 744 073 709 551 615)
            FloatType  =  9, // 7 digits (±1.5e−45 to ±3.4e38)
            DoubleType = 10  // 15-16 digits (±5.0e−324 to ±1.7e308)
        }

        private static Dictionary<Type, T> typeEnums
            = new Dictionary<Type, T>
                {
                    { typeof(sbyte ), T.SByteType  },
                    { typeof(byte  ), T.ByteType   },
                    { typeof(char  ), T.CharType   },
                    { typeof(short ), T.ShortType  },
                    { typeof(ushort), T.UShortType },
                    { typeof(int   ), T.IntType    },
                    { typeof(uint  ), T.UIntType   },
                    { typeof(long  ), T.LongType   },
                    { typeof(ulong ), T.ULongType  },
                    { typeof(float ), T.FloatType  },
                    { typeof(double), T.DoubleType }
                };

        private static T[][] targetType = new T[][]
        {
            //                        SByteType ,   ByteType  ,   CharType  ,   ShortType ,   UShortType,   IntType   ,   UIntType  ,   LongType  ,   ULongType ,   FloatType ,   DoubleType
            /*SByteType */new T[] { T.SByteType , T.ShortType , T.IntType   , T.ShortType , T.IntType   , T.IntType   , T.LongType  , T.LongType  , T.LongType  , T.FloatType , T.DoubleType },
            /*ByteType  */new T[] { T.ShortType , T.ByteType  , T.UShortType, T.ShortType , T.UShortType, T.IntType   , T.UIntType  , T.LongType  , T.ULongType , T.FloatType , T.DoubleType },
            /*CharType  */new T[] { T.IntType   , T.UShortType, T.CharType  , T.IntType   , T.UShortType, T.IntType   , T.LongType  , T.LongType  , T.ULongType , T.FloatType , T.DoubleType },
            /*ShortType */new T[] { T.ShortType , T.ShortType , T.IntType   , T.ShortType , T.IntType   , T.IntType   , T.LongType  , T.LongType  , T.LongType  , T.FloatType , T.DoubleType },
            /*UShortType*/new T[] { T.IntType   , T.UShortType, T.UShortType, T.IntType   , T.UShortType, T.IntType   , T.UIntType  , T.LongType  , T.ULongType , T.FloatType , T.DoubleType },
            /*IntType   */new T[] { T.IntType   , T.IntType   , T.IntType   , T.IntType   , T.IntType   , T.IntType   , T.LongType  , T.LongType  , T.LongType  , T.FloatType , T.DoubleType },
            /*UIntType  */new T[] { T.LongType  , T.UIntType  , T.LongType  , T.LongType  , T.UIntType  , T.LongType  , T.UIntType  , T.LongType  , T.ULongType , T.FloatType , T.DoubleType },
            /*LongType  */new T[] { T.LongType  , T.LongType  , T.LongType  , T.LongType  , T.LongType  , T.LongType  , T.LongType  , T.LongType  , T.LongType  , T.FloatType , T.DoubleType },
            /*ULongType */new T[] { T.LongType  , T.ULongType , T.ULongType , T.LongType  , T.ULongType , T.LongType  , T.ULongType , T.LongType  , T.ULongType , T.FloatType , T.DoubleType },
            /*FloatType */new T[] { T.FloatType , T.FloatType , T.FloatType , T.FloatType , T.FloatType , T.FloatType , T.FloatType , T.FloatType , T.FloatType , T.FloatType , T.DoubleType },
            /*DoubleType*/new T[] { T.DoubleType, T.DoubleType, T.DoubleType, T.DoubleType, T.DoubleType, T.DoubleType, T.DoubleType, T.DoubleType, T.DoubleType, T.DoubleType, T.DoubleType }
        };

        private T GetTypeEnum(object value)
        {
            return GetTypeEnum(value.GetType());
        }

        private T GetTypeEnum(Type type)
        {
            try
            {
                return typeEnums[type];
            }
            catch
            {
                throw new NotSupportedException(
                    string.Format("Unsupported data type {0}.", type));
            }
        }

        private Type GetType(T typeEnum)
        {
            switch(typeEnum)
            {
                case T.SByteType : return typeof(sbyte );
                case T.ByteType  : return typeof(byte  );
                case T.CharType  : return typeof(char  );
                case T.ShortType : return typeof(short );
                case T.UShortType: return typeof(ushort);
                case T.IntType   : return typeof(int   );
                case T.UIntType  : return typeof(uint  );
                case T.LongType  : return typeof(long  );
                case T.ULongType : return typeof(ulong );
                case T.FloatType : return typeof(float );
                case T.DoubleType: return typeof(double);
                default:
                    throw new NotSupportedException(
                        string.Format("Unsupported data type {0}.", typeEnum));
            }
        }

        private object ConvertValue(object value, T targetTypeEnum)
        {
            switch(targetTypeEnum)
            {
                case T.SByteType : return Convert.ToSByte (value);
                case T.ByteType  : return Convert.ToByte  (value);
                case T.CharType  : return Convert.ToChar  (value);
                case T.ShortType : return Convert.ToInt16 (value);
                case T.UShortType: return Convert.ToUInt16(value);
                case T.IntType   : return Convert.ToInt32 (value);
                case T.UIntType  : return Convert.ToUInt32(value);
                case T.LongType  : return Convert.ToInt64 (value);
                case T.ULongType : return Convert.ToUInt64(value);
                case T.FloatType : return Convert.ToSingle(value);
                case T.DoubleType: return Convert.ToDouble(value);
                default:
                    throw new NotSupportedException(
                        string.Format("Unsupported data type {0}.", targetTypeEnum));
            }
        }
    }
}
