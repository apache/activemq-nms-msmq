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

namespace Apache.NMS.Selector
{
    /// <summary>
    /// An expression which negates a numeric expression value.
    /// </summary>
    public class NegateExpression : UnaryExpression
    {
        protected override string ExpressionSymbol
        {
            get { return "-"; }
        }

        public NegateExpression(IExpression left)
            : base(left)
        {
        }

        public override object Evaluate(MessageEvaluationContext message)
        {
            object rvalue = Right.Evaluate(message);
            if(rvalue == null   ) return null;
            if(rvalue is int    ) return -(int    )rvalue;
            if(rvalue is long   ) return -(long   )rvalue;
            if(rvalue is double ) return -(double )rvalue;
            if(rvalue is float  ) return -(float  )rvalue;
            if(rvalue is decimal) return -(decimal)rvalue;
            if(rvalue is short  ) return -(short  )rvalue;
            if(rvalue is byte   ) return -(byte   )rvalue;
            return null;
        }
    }
}
