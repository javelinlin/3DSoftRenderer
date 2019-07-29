// jave.lin 2019.07.18
using SoftRenderer.Common.Mathes;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace RendererCommon.SoftRenderer.Common.Shader
{
    [Description("Shader的共享数据基类")]
    public class BasicShaderData : IDisposable // default
    {
        public List<Vector4> Datas { get; private set; }
        public int readIdx = 0;
        public int writeIdx = 0;

        public BasicShaderData()
        {
            Datas = new List<Vector4>();
        }

        public void WriteVec(Vector4 vec)
        {
            if(writeIdx < Datas.Count)
            {
                Datas[writeIdx++] = vec;
            }
            else
            {
                Datas.Add(vec);
            }
        }

        public void WriteVec(Vector4[] vecs)
        {
            foreach (var vec in vecs)
            {
                WriteVec(vec);
            }
        }

        public void WriteMat(Matrix4x4 mat)
        {
            WriteVec(mat.GetCol(0));
            WriteVec(mat.GetCol(1));
            WriteVec(mat.GetCol(2));
            WriteVec(mat.GetCol(3));
        }

        public void WriteMat(Matrix4x4[] mats)
        {
            foreach (var m in mats)
            {
                WriteMat(m);
            }
        }

        public void WriteColor(ColorNormalized color)
        {
            WriteVec(color);
        }

        public void WriteColor(ColorNormalized[] colors)
        {
            foreach (var c in colors)
            {
                WriteVec(c);
            }
        }

        public Matrix4x4 ReadMat()
        {
            var mat = new Matrix4x4(
                Datas[readIdx++],
                Datas[readIdx++],
                Datas[readIdx++],
                Datas[readIdx++]);
            return mat;
        }

        public void ReadMat(ref Matrix4x4 mat)
        {
            mat.SetCol(readIdx, Datas[readIdx++]);
            mat.SetCol(readIdx, Datas[readIdx++]);
            mat.SetCol(readIdx, Datas[readIdx++]);
            mat.SetCol(readIdx, Datas[readIdx++]);
        }

        public Matrix4x4[] ReadMats(int num)
        {
            Matrix4x4[] mats = new Matrix4x4[num];
            for (int i = 0; i < num; i++)
            {
                mats[i] = ReadMat();
            }
            return mats;
        }

        public void ReadMats(int num, ref Matrix4x4[] mats)
        {
            for (int i = 0; i < num; i++)
            {
                mats[i] = ReadMat();
            }
        }

        public Vector4 ReadVec()
        {
            return Datas[readIdx++];
        }

        public void ReadVec(ref Vector4 vec)
        {
            var v = Datas[readIdx++];
            vec.x = v.x;
            vec.y = v.y;
            vec.z = v.z;
            vec.w = v.w;
        }

        public Vector4[] ReadVecs(int num)
        {
            Vector4[] mats = new Vector4[num];
            for (int i = 0; i < num; i++)
            {
                mats[i] = ReadVec();
            }
            return mats;
        }

        public void ReadVecs(int num, ref Vector4[] vecs)
        {
            for (int i = 0; i < num; i++)
            {
                vecs[i] = ReadVec();
            }
        }

        public ColorNormalized ReadColor()
        {
            return Datas[readIdx++];
        }

        public void ReadColor(ref ColorNormalized color)
        {
            var c = Datas[readIdx++];
            color.r = c.x;
            color.g = c.y;
            color.b = c.z;
            color.a = c.w;
        }

        public ColorNormalized[] ReadColors(int num)
        {
            ColorNormalized[] mats = new ColorNormalized[num];
            for (int i = 0; i < num; i++)
            {
                mats[i] = ReadColor();
            }
            return mats;
        }

        public void ReadColors(int num, ref ColorNormalized[] colors)
        {
            for (int i = 0; i < num; i++)
            {
                colors[i] = ReadColor();
            }
        }

        public bool HasData(int size)
        {
            return (readIdx + size) < Datas.Count;
        }

        // 数据转换
        public virtual void Translation()
        {

        }

        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);

            if (Datas != null)
            {
                Datas.Clear();
                Datas = null;
            }
        }
    }
    [Description("自己扩展的Shader共享数据，类似Uniform block")]
    public class ShaderData : BasicShaderData // mine ext
    {
        //instancing data
        //public Matrix4x4 ObjToWorldMat;         // model/world mat
        //public Matrix4x4 WorldToViewMat;        // view/eye/camera mat
        //public Matrix4x4 ViewToProjectMat;      // cip/proj mat
        //public Matrix4x4 MVP;                   // proj * view * model mat

        // global data
        public Vector4 CameraPos;
        public Vector4 CameraParams; // x=near,y=far,z=unused,w=unused

        public ColorNormalized Ambient;

        public Vector4[] LightPos;
        public ColorNormalized[] LightColor;
        public Vector4[] LightItensity;
        public Vector4[] LightParams1;

        public ShaderData(int maxLight)
        {
            //ObjToWorldMat = Matrix4x4.GetMat();
            //WorldToViewMat = Matrix4x4.GetMat();
            //ViewToProjectMat = Matrix4x4.GetMat();
            //MVP = Matrix4x4.GetMat();

            CameraPos = new Vector4();

            Ambient = new ColorNormalized();

            LightPos = new Vector4[maxLight];
            LightColor = new ColorNormalized[maxLight];
            LightItensity = new Vector4[maxLight];
            LightParams1 = new Vector4[maxLight];
        }

        public override void Translation()
        {
            // 暂时不用这种方式
            // 外部就直接设置吧
            readIdx = 0;

            //ReadMat(ref ObjToWorldMat);
            //ReadMat(ref WorldToViewMat);
            //ReadMat(ref ViewToProjectMat);
            //ReadMat(ref MVP);

            if (!HasData(4)) return;
            ReadVec(ref CameraPos);
            if (!HasData(4)) return;
            ReadVec(ref CameraParams);
            if (!HasData(4)) return;
            ReadColor(ref Ambient);

            var len = LightPos.Length;
            if (!HasData(4*len)) return;
            ReadVecs(len, ref LightPos);
            if (!HasData(4*len)) return;
            ReadVecs(len, ref LightPos);
            if (!HasData(4*len)) return;
            ReadVecs(len, ref LightPos);
            if (!HasData(4*len)) return;
            ReadVecs(len, ref LightPos);
        }

        public void NowDataWriteToBuff()
        {
            writeIdx = 0;
            Datas.Clear();
            WriteVec(CameraPos);
            WriteVec(CameraParams);
            WriteColor(Ambient);
            WriteVec(LightPos);
            WriteColor(LightColor);
            WriteVec(LightItensity);
            WriteVec(LightParams1);
        }

        public override void Dispose()
        {
            GC.SuppressFinalize(this);

            if (LightPos != null)
            {
                Array.Resize(ref LightPos, 0);
                LightPos = null;
            }
            if (LightColor != null)
            {
                Array.Resize(ref LightColor, 0);
                LightColor = null;
            }
            if (LightItensity != null)
            {
                Array.Resize(ref LightItensity, 0);
                LightItensity = null;
            }
            if (LightParams1 != null)
            {
                Array.Resize(ref LightParams1, 0);
                LightParams1 = null;
            }
            base.Dispose();
        }
    }
}
