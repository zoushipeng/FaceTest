using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCardSystem.Recognizer
{
    public struct AFR_FSDK_FACEMODEL
    {
        public IntPtr pbFeature;	// The extracted features

        public int lFeatureSize;	// The size of pbFeature
    }
}
