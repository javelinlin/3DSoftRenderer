// jave.lin 2019.07.21
using System;

namespace RendererCoreCommon.Renderer.Common.Attributes
{
    internal static class ShaderUtil
    {
        public const int MAX_NUM_REGISTER = 8;
    }
    /*=========UTILS START=========*/
    public static class NameUtil
    {
        public static int HashID(string str)
        {
            return str.GetHashCode();
        }
    }
    /*=========UTILS END=========*/

    /*=========COMMON START=========*/

    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class NointerpolationAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class UniformAttribute : Attribute { }
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class InAttribute : Attribute { }
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class OutAttribute : Attribute { }
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class NameAttribute : Attribute { }
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class NameHashAttribute : Attribute { }
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class PositionAttribute : Attribute { }
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class ColorAttribute : Attribute
    {
        public byte Location { get; }
        public ColorAttribute(byte location = 0)
        {
            if (location > ShaderUtil.MAX_NUM_REGISTER)
            {
                throw new Exception($"最多{ShaderUtil.MAX_NUM_REGISTER}个{GetType().Name}");
            }
            this.Location = location;
        }
    }
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class TexcoordAttribute : Attribute
    {
        public byte Location { get; }
        public TexcoordAttribute(byte location = 0)
        {
            if (location > ShaderUtil.MAX_NUM_REGISTER)
            {
                throw new Exception($"最多{ShaderUtil.MAX_NUM_REGISTER}个{GetType().Name}");
            }
            this.Location = location;
        }
    }
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class NormalAttribute : Attribute
    {
        public byte Location { get; }
        public NormalAttribute(byte location = 0)
        {
            if (location > ShaderUtil.MAX_NUM_REGISTER)
            {
                throw new Exception($"最多{ShaderUtil.MAX_NUM_REGISTER}个{GetType().Name}");
            }
            this.Location = location;
        }
    }
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class TangentAttribute : Attribute
    {
        public byte Location { get; }
        public TangentAttribute(byte location = 0)
        {
            if (location > ShaderUtil.MAX_NUM_REGISTER)
            {
                throw new Exception($"最多{ShaderUtil.MAX_NUM_REGISTER}个{GetType().Name}");
            }
            this.Location = location;
        }
    }


    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ShaderAttribute : Attribute { }
    /*=========COMMON END=========*/

    /*=========VS START=========*/
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class SV_PositionAttribute : Attribute { }
    /*=========VS END=========*/

    /*=========FS START=========*/
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class SV_TargetAttribute : Attribute
    {
        public byte Location { get; }
        public SV_TargetAttribute(byte location = 0)
        {
            if (location > ShaderUtil.MAX_NUM_REGISTER)
            {
                throw new Exception($"最多{ShaderUtil.MAX_NUM_REGISTER}个{GetType().Name}");
            }
            this.Location = location;
        }
    }
    /*=========FS END=========*/
}
