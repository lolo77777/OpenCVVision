# OpencvVision

## 是什么？

这是一个使用.net 6基于OpencvSharp、ReactiveUI开发的自用工具，主要用来做ReactiveUI与OpencvSharp学习过程中的尝试；根据个人需求不定期更新添加功能，欢迎讨论交流。

### ！其他信息请参考项目OpenCVVision.View内AppBootstrapper.cs文件里的相关说明

## 引用

| 功能          | 引用包                                                                                                |
| ----------- | -------------------------------------------------------------------------------------------------- |
| OpencvSharp | [OpencvSharp](https://github.com/shimat/opencvsharp_samples)                                       |
| MVVMk框架     | [ReactiveUI](https://github.com/reactiveui/ReactiveUI)                                             |
| 界面控件        | [MaterialDesignInXamlToolkit](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit) |
| 标题栏         | [MahApps.Metro](https://github.com/MahApps/MahApps.Metro)                                          |
| 响应式集合       | [DynamicData](https://github.com/reactivemarbles/DynamicData)                                      |
| 返回结果包装      | [FluentResults](https://github.com/altmann/FluentResults)                                          |

## 推荐

> 其它项目： 
> [OpenCVSharpHelper](https://gitee.com/tfarcraw/opencvsharphelper)

## 部分功能截图

### 22/09/08更新2

从朋友处了解到基恩士的浓淡补正算法，感觉挺有意思，查找了下相关的资料，尝试简单的实现一下明暗局部的筛选。思路是选取一个卷积核，进行滑移，规则是先排列挑出核内部的中值，在通过设置删除亮暗比例设置中值附近的阈值，来筛选出局部过亮或过暗的部分；可以通过卷积核长宽设置方向，来实现特定方向的筛选，示意如下图：
筛选出不均匀的边缘
![筛选边缘](Img/23.png)
筛选出中间的过亮部分和过暗部分
![筛选暗点亮点](Img/24.png)
沿着Y方向，筛选出过亮部分，因为选定了方向，条纹不会被选出,仅选出非条纹的亮线
![Y方向筛选](Img/25.png)

### 22/09/08更新1

调整解决方案项目目录，新增服务子项目
添加海康，大恒相机采集图像。需要自行安装对应品牌SDK。由以往项目中使用程序汇总而来，暂时不具备测试条件，等待后续测试硬件。
添加信息栏，根据日志等级变换背景颜色
![信息栏默认](Img/20.png)
![信息栏错误](Img/21.png)
添加日志面板，点击信息栏右侧按钮展开，点击其它空白区域丢失焦点关闭
![日志面板](Img/22.png)

### 22/06/21更新

微信二维码检测解码
![18](Img/18.jpg)

![19](Img/19.jpg)

### 22/06/20更新

PaddleX图像分类推理
![16](Img/16.jpg)
PaddleX目标检测推理
![17](Img/17.jpg)

### 功能

首页
![00](Img/00.jpg)
夜间模式添加图片
![02](Img/02.jpg)
ROI
![03](Img/03.jpg)
色彩空间
![04](Img/04.jpg)
滤波
![05](Img/05.jpg)
二值化
![06](Img/06.jpg)
形态学
![07](Img/07.jpg)
连通域
![08](Img/08.jpg)
轮廓
![09](Img/09.jpg)
格雷码条纹结构光
![10](Img/10.jpg)
灰度质心线激光
![11](Img/11.jpg)
光度立体法求表面法线与深度图
![12](Img/12.jpg)
Yolo识别
![13](Img/13.jpg)
特征点匹配
![14](Img/14.jpg)
