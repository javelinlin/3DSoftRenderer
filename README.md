# 3DSoftRenderer

blog:https://blog.csdn.net/linjf520/article/details/96047240

这是个学习项目。
本人一边学习图形相关内容，一边将总结的知识写入该项目中实现。
为了增加对图形方面的经验而写的软渲染流程。
代码有很大的优化空间。

# 遗留问题
- 投影矩阵：未能完全理解数值的推导
- 透视校正：使用了1/z线性变换因子来处理，但是结果还是不对，主要对投影矩阵未能基准理解
  - 插值前因数相乘：1/z，因为投影空间中的1/z值还是线性变换的
  - 生成边框信息:
    - GenLineFragInfos(FragInfo f0, FragInfo f1, List<FragInfo> result)
    - 
      var invz0 = 1/f0.p.w;
      var invz1 = 1/f1.p.w;
      foreach(var f in result) {
        f.z = 1/lerp(invz0, invz1, t);
        foreach(var interpolationData in f.datas) {
          // 顶点输出需要插值的数据
          interpolationData = f.z * (lerp(interpolationData * invz0, interpolationData * invz1, t));
        }
      }
    - 但是结果还是不对

# 主要现实的功能
- 顶点变换
  - obj2world : obj pos
  - world2view : world pos
  - view2proj : clip pos
  - clip2ndc : ndc pos
  - ndc2win : win pos
- 栅格化：线、三角形
- 简单封装GameObject,Mesh,Camera,Material
- FrontFace 决定正背面
- CullFace 提出面向
- Scissor 裁剪矩形
- Depth offset 深度偏移
- DepthTest 更像是Early-z，因为再fragment shader前测试
- AlphaTest Alpha测试
- Blend 混合
- Similar Programmable pipeline 没有并行的可编程管线
  - Vertex Shader
  - Fragment Shader
- Texture2D & Sampler2D(WrapMode, FilterMode(only point))
