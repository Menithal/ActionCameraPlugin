/**  Copyright 2020 Matti 'Menithal' Lahtinen

* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*    http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
* 
**/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MACPlugin.Utility
{

    public interface Constraint<T>
    {
        T Constrain(T A);
    }
    public class FloatConstraint : Constraint<float>
    {
        public float DefaultValue { get; private set; }
        public float min;
        public float max;

        public FloatConstraint(float defaultValue, float min, float max)
        {
            this.DefaultValue = defaultValue;
            this.min = min;
            this.max = max;
        }
        public FloatConstraint(float defaultValue, float max)
        {
            this.DefaultValue = defaultValue;
            this.min = -max;
            this.max = max;
        }

        public float Constrain(float value)
        {
            if (value <= this.min) return this.min;
            if (value >= this.max) return this.max;

            return value;
        }
    }

    public class IntConstraint : Constraint<int>
    {
        public int DefaultValue { get; private set; }
        public int min;
        public int max;

        public IntConstraint(int defaultValue, int min, int max)
        {
            this.DefaultValue = defaultValue;
            this.min = min;
            this.max = max;
        }
        public IntConstraint(int defaultValue, int max)
        {
            this.DefaultValue = defaultValue;
            this.min = -max;
            this.max = max;
        }

        public int Constrain(int value)
        {
            if (value <= this.min) return this.min;
            if (value >= this.max) return this.max;

            return value;
        }
    }

    public class BooleanConstraint : Constraint<bool>
    {
        public bool DefaultValue { get; private set; }


        public BooleanConstraint(bool defaultValue)
        {
            this.DefaultValue = defaultValue;
        }

        public bool Constrain(bool A)
        {
            return A;
        }
    }


    public abstract class SerializableConfig : System.Attribute
    {
    }


    [AttributeUsage(AttributeTargets.Field)]
    public class SerializableBooleanConfig : SerializableConfig
    {
        public BooleanConstraint Constraint { get; private set; }
        public SerializableBooleanConfig(bool defaultValue)
        {
            this.Constraint = new BooleanConstraint(defaultValue);
        }
    }


    [AttributeUsage(AttributeTargets.Field)]
    public class SerializableFloatConfig : SerializableConfig
    {
        public FloatConstraint Constraint { get; private set; }
        public SerializableFloatConfig(float defaultValue, float min, float max)
        {
            this.Constraint = new FloatConstraint(defaultValue, min, max);
        }

        public SerializableFloatConfig(float defaultValue, float max)
        {
            this.Constraint = new FloatConstraint(defaultValue, max);
        }
    }


    [AttributeUsage(AttributeTargets.Field)]
    public class SerializableIntConfig : SerializableConfig
    {
        public IntConstraint Constraint { get; private set; }
        public SerializableIntConfig(int defaultValue, int min, int max)
        {
            this.Constraint = new IntConstraint(defaultValue, min, max);
        }

        public SerializableIntConfig(int defaultValue, int max)
        {
            this.Constraint = new IntConstraint(defaultValue, max);
        }
    }
}
