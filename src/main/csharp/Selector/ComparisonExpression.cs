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
using System.Collections;

namespace Apache.NMS.Selector
{
    /// <summary>
    /// A filter performing a comparison of two or more expressions or objects.
    /// </summary>
    public abstract class ComparisonExpression : BinaryExpression, IBooleanExpression
    {
        public ComparisonExpression(IExpression left, IExpression right)
            : base(left, right)
        {
        }

        public override object Evaluate(MessageEvaluationContext message)
        {
            object lvalue = Left.Evaluate(message);
            object rvalue = Right.Evaluate(message);

            int? compared = null;

            if(lvalue == null || rvalue == null)
            {
                if(lvalue == null && rvalue == null)
                {
                    compared = 0;
                }
            }
            else
            {
                if(lvalue == rvalue)
                {
                    compared = 0;
                }
                else if(lvalue is string && rvalue is string)
                {
                    compared = ((string)lvalue).CompareTo(rvalue);
                }
                else
                {
                    AlignedNumericValues values = new AlignedNumericValues(lvalue, rvalue);

                    switch(values.TypeEnum)
                    {
                        case AlignedNumericValues.T.SByteType : compared = ((sbyte )values.Left).CompareTo((sbyte )values.Right); break;
                        case AlignedNumericValues.T.ByteType  : compared = ((byte  )values.Left).CompareTo((byte  )values.Right); break;
                        case AlignedNumericValues.T.CharType  : compared = ((char  )values.Left).CompareTo((char  )values.Right); break;
                        case AlignedNumericValues.T.ShortType : compared = ((short )values.Left).CompareTo((short )values.Right); break;
                        case AlignedNumericValues.T.UShortType: compared = ((ushort)values.Left).CompareTo((ushort)values.Right); break;
                        case AlignedNumericValues.T.IntType   : compared = ((int   )values.Left).CompareTo((int   )values.Right); break;
                        case AlignedNumericValues.T.UIntType  : compared = ((uint  )values.Left).CompareTo((uint  )values.Right); break;
                        case AlignedNumericValues.T.LongType  : compared = ((long  )values.Left).CompareTo((long  )values.Right); break;
                        case AlignedNumericValues.T.ULongType : compared = ((ulong )values.Left).CompareTo((ulong )values.Right); break;
                        case AlignedNumericValues.T.FloatType : compared = ((float )values.Left).CompareTo((float )values.Right); break;
                        case AlignedNumericValues.T.DoubleType: compared = ((double)values.Left).CompareTo((double)values.Right); break;
                    }
                }
            }

            return AsBoolean(compared);
        }
    
        public abstract bool AsBoolean(int? compared);

        public bool Matches(MessageEvaluationContext message)
        {
            object value = Evaluate(message);
            return value != null && (bool)value;            
        }

        // Equality expressions
        public static IBooleanExpression CreateEqual(IExpression left, IExpression right)
        {
    	    return new EqualExpression(left, right, true);
        }

        public static IBooleanExpression CreateNotEqual(IExpression left, IExpression right)
        {
            return new EqualExpression(left, right, false);
        }

        public static IBooleanExpression CreateIsNull(IExpression left)
        {
            return new IsNullExpression(left, true);
        }

        public static IBooleanExpression CreateIsNotNull(IExpression left)
        {
            return new IsNullExpression(left, false);
        }

        // Binary comparison expressions
        public static IBooleanExpression CreateGreaterThan(IExpression left, IExpression right)
        {
    	    return new GreaterExpression(left, right);
        }

        public static IBooleanExpression CreateGreaterThanOrEqual(IExpression left, IExpression right)
        {
    	    return new GreaterOrEqualExpression(left, right);
        }

        public static IBooleanExpression CreateLesserThan(IExpression left, IExpression right)
        {
    	    return new LesserExpression(left, right);
        }

	    public static IBooleanExpression CreateLesserThanOrEqual(IExpression left, IExpression right)
        {
    	    return new LesserOrEqualExpression(left, right);
        }

        // Other comparison expressions
        public static IBooleanExpression CreateLike(IExpression left, string right, string escape)
        {
            return new LikeExpression(left, right, escape, true);
        }

        public static IBooleanExpression CreateNotLike(IExpression left, string right, string escape)
        {
            return new LikeExpression(left, right, escape, false);
        }

        public static IBooleanExpression CreateBetween(IExpression value, IExpression left, IExpression right)
        {
            return LogicExpression.CreateAND(CreateGreaterThanOrEqual(value, left), CreateLesserThanOrEqual(value, right));
        }

        public static IBooleanExpression CreateNotBetween(IExpression value, IExpression left, IExpression right)
        {
            return LogicExpression.CreateOR(CreateLesserThan(value, left), CreateGreaterThan(value, right));
        }

        public static IBooleanExpression CreateIn(IExpression left, ArrayList elements)
        {
            return new InExpression(left, elements, true);
        }

        public static IBooleanExpression CreateNotIn(IExpression left, ArrayList elements)
        {
            return new InExpression(left, elements, false);
        }
    }
}
