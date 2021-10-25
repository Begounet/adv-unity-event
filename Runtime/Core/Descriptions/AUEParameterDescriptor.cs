using System;

namespace AUE.Descriptors
{
    public class AUEParameterDescriptor
    {
        public Type ParameterType { get; set; }
        public AUEMethodParameterInfo.EMode Mode { get; set; }
        public IAUECustomArgument CustomArgument { get; set; }

        public AUEParameterDescriptor(AUEMethodParameterInfo.EMode mode, Type parameterType, IAUECustomArgument customArgument)
        {
            Mode = mode;
            CustomArgument = customArgument;
            ParameterType = parameterType;
        }
    }
}