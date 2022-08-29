/*
 * Copyright (C) 2021 The Authors of CEL-Java
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace Cel.Common.Types.Traits
{
    public enum Trait
    {
        None,

        /// <summary>
        /// AdderType types provide a '+' operator overload. </summary>
        AdderType,

        /// <summary>
        /// ComparerType types support ordering comparisons '&lt;', '&lt;=', '&lt;', '&lt;='. </summary>
        ComparerType,

        /// <summary>
        /// ContainerType types support 'in' operations. </summary>
        ContainerType,

        /// <summary>
        /// DividerType types support '/' operations. </summary>
        DividerType,

        /// <summary>
        /// FieldTesterType types support the detection of field value presence. </summary>
        FieldTesterType,

        /// <summary>
        /// IndexerType types support index access with dynamic values. </summary>
        IndexerType,

        /// <summary>
        /// IterableType types can be iterated over in comprehensions. </summary>
        IterableType,

        /// <summary>
        /// IteratorType types support iterator semantics. </summary>
        IteratorType,

        /// <summary>
        /// MatcherType types support pattern matching via 'matches' method. </summary>
        MatcherType,

        /// <summary>
        /// ModderType types support modulus operations '%' </summary>
        ModderType,

        /// <summary>
        /// MultiplierType types support '*' operations. </summary>
        MultiplierType,

        /// <summary>
        /// NegatorType types support either negation via '!' or '-' </summary>
        NegatorType,

        /// <summary>
        /// ReceiverType types support dynamic dispatch to instance methods. </summary>
        ReceiverType,

        /// <summary>
        /// SizerType types support the size() method. </summary>
        SizerType,

        /// <summary>
        /// SubtractorType type support '-' operations. </summary>
        SubtractorType
    }
}