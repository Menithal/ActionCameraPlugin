using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActionCamera.Utility
{
   
    // Work in progress
    public class ConfigFloatConstraint: System.Attribute
    {
        public float min { get; private set; }
        public float max;

        public ConfigFloatConstraint(float min, float max)
        {
            this.min = min;
            this.max = max;
        }
        public ConfigFloatConstraint(float max) {
            this.min = -max;
            this.max = max;
        }


    }
}
