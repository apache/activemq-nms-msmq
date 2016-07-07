using System;
/**
 *
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace Apache.NMS.Selector
{
    /// <summary>
    /// A filter performing an addition of two expressions.
    /// </summary>
    public class PlusExpression : ArithmeticExpression
    {
        protected override string ExpressionSymbol
        {
            get { return "+"; }
        }

        public PlusExpression(IExpression left, IExpression right)
            : base(left, right)
        {
        }

        public override object Evaluate(MessageEvaluationContext message)
        {
            object lvalue = Left.Evaluate(message);
            if(lvalue == null) return null;

            object rvalue = Right.Evaluate(message);
            if(lvalue is string) return (string)lvalue + rvalue;
            if(rvalue == null) return null;

            AlignedNumericValues values = new AlignedNumericValues(lvalue, rvalue);

            object result = null;

            switch(values.TypeEnum)
            {
                case AlignedNumericValues.T.SByteType : result = (sbyte )values.Left + (sbyte )values.Right; break;
                case AlignedNumericValues.T.ByteType  : result = (byte  )values.Left + (byte  )values.Right; break;
                case AlignedNumericValues.T.CharType  : result = (char  )values.Left + (char  )values.Right; break;
                case AlignedNumericValues.T.ShortType : result = (short )values.Left + (short )values.Right; break;
                case AlignedNumericValues.T.UShortType: result = (ushort)values.Left + (ushort)values.Right; break;
                case AlignedNumericValues.T.IntType   : result = (int   )values.Left + (int   )values.Right; break;
                case AlignedNumericValues.T.UIntType  : result = (uint  )values.Left + (uint  )values.Right; break;
                case AlignedNumericValues.T.LongType  : result = (long  )values.Left + (long  )values.Right; break;
                case AlignedNumericValues.T.ULongType : result = (ulong )values.Left + (ulong )values.Right; break;
                case AlignedNumericValues.T.FloatType : result = (float )values.Left + (float )values.Right; break;
                case AlignedNumericValues.T.DoubleType: result = (double)values.Left + (double)values.Right; break;
            }

            return result;
        }
    }
}
