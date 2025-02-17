# AOG v0.2 用例



## Application



### aog_app_dev: 基于AOG的应用程序开发

1. 应用程序加入对aog_checker的依赖。例如，C/C++ Windows程序中，`include "aog_check.h"` 并且 link with `aog_checker.dll`

2. 应用程序初始化过程中，调用`aog_init()` 函数

3. 应用程序添加对所需AI服务的声明，即添加`.aog`文件。文件内部为`JSON`格式，示例如下，该应用程序希望AOG提供默认的chat服务，以及基于模型`stable-diffusion-1.5-int4`  的 `text-to-image` 服务。

   * 注意：某些AI服务可能需要使用到多个模型，需要为每个都指定名字，例如下面为text-to-image指定的base

   ```json
   {
       "version": "0.2";
       "service": [{
           "chat": true,
           "text-to-image": {
               "models": {
                   "base": "stable-diffusion-1.5-int4"
               }
       }]
   }
   ```

4. 应用程序内部使用RESTful API的方式，通过AOG服务端口（例如`http://localhost:16688/aog/...`），按照相关AOG specification，调用以上声明的AOG相关的AI服务。

5. 应用程序最终打包时，应包含`aog_check.dll`和`.aog`文件



### aog_app_run: 基于AOG的应用程序的运行

1. 应用程序启动

2. 初始化过程调用`aog_init()` 函数

3. `aog_init()` 检查本地是否已经安装 `aog core` （aog api layer 以及 aog manager)

    * 如果尚未安装，弹出说明文件，请用户按照指示手动下载安装。安装之后点确认，应用继续执行
    * 未来安装应该自动，用户只需要同意确认即可
    * 如果用户不同意安装，则程序退出

4. `aog_init()` 查看`aog` 是否启动

    * 如没有启动，则`aog_init()` 需要启动`aog_core`

5. `aog_init()` 把本应用的`.aog`的内容提交给`aog_core`

    * 提交方式应该同样是通过aog提供的一个RESTful API (即访问)

    * aog core 查看里面声明需要的AI服务是否已经满足

    * 如果需要的AI服务尚未满足

      

### aog_sample_app: AOG 样例程序

1. 程序主体应该是 c/c++或者 c#的，使用 aog_checker

2. 程序启动后是一个简单的 chat QnA应用 （未来加入文生图）

3. 界面首页应该有相关配置选项 - 方便展示和测试各种不同的 API参数

   * hybrid_policy: default, always remote, always local
   * stream: yes / no

   



## AOG Manager (Control Panel)



### aog_install_service_model: AOG 安装 AI Service及模型

1. 通过aog api layer及发送RESTful API请求，来触发安装命令

   * 例如`aog_init()` 发送请求，参见`aog_app_run 用例`
   * 又如 `aog CLI` 

2. 若请求安装service，以 chat 服务为例，则

   * 获得本机相关配置( CPU / GPU / NPU 型号，内存显存大小)
   * 联网提交本机配置并获得推荐的安装内容（包括引擎和默认的模型，及相关下载地址）
   * 确认本地是否已经安装了推荐的引擎和模型
     * 若本地尚未相关引擎和模型
     * 弹出提示，获取用户安装许可
     * 下载相关安装包并安装
   * 启动安装好的AI服务，纳入aog 管理
   * Q1 只需要实现以上流程的弱化版本。即联网推荐基本hard code
     * 只是检查机器是MTL / LNL，内存>=16GB
     * 只是安装 chat 服务，且只推荐IPEX-LLM优化过的 ollama，默认使用某个模型

3. 若请求为某个服务安装某个模型，例如为 chat 服务安装llama3.1

   * 与上述步骤类似。联网提交的时候加入了模型要求。推荐结果返回模型下载地址，以及推荐的支持这个模型的引擎

   * Q1 的弱化实现是 hard code的，所谓的联网推荐都基于 ollama

     



## AOG CLI

命令行工具，具体待设计，主要包括以下功能（其中相当多的功能是通过给aog api layer发送RESTful 请求来完成）：

* start / stop aog
* service 和 model 的下载安装
* 枚举当前的状态 - aog 状态，各个 service / model 的状态
* log 和 debug 等辅助功能



## AOG API Layer

### aog_invoke_cloud: 调用云端AI服务

1. 对某次aog api layer的 AI服务的调用 触发对云端AI服务调用
   * 查看RESTful调用里面是否指定了 hybrid_policy，如果没有指定，则使用系统默认的hybrid_policy
   * 对于default policy，根据CPU/GPU 负载确定是否调用云端AI而非本地AI
   * 对于always remote policy，直接使用云端
2. 获得准备调用的云端服务的具体信息
   * 当前是查看 `opac.json` 里面的定义，例如 `chat service`的 `remote`字段所指向的service provider
   * 未来这一步可能是需要动态的，因为用户可能会动态的改配置
   * 具体信息包括 URL，API Flavor，以及api key 
     * 目前api key 定义在`opac.json`的具体`service provider`的`extra_headers`里面
     * 注意：这里可能需要额外的设计，来表明一个云端ai是否需要api key，具体的header是什么，获得方式是什么等
3. 获取对应服务的API Key （如果需要调用的云端AI服务需要api key，但是用户/本机还没有设定 ）
   * 需要弹出具体的说明，引导用户去申请相关的api key
   * 用户申请到api key之后，提供一个方式把这个key 记录进 aog （暂时可能改动 `opac.json`，未来改进）
     * 考虑的方式是弹出html页面让用户输入，输入后该html页面调用aog的 RESTful API写入api key
4. 按照获得的信息调用云端服务
   * 使用URL, API Flavor 以及 api key 等，调用相关Cloud AI
     * 加入对https的支持（目前aog针对 http做过测试，尚不清楚 https是否需要有额外工作）



### aog_popular_cloud_ai: 常见云端AI服务的支持

1. 对 腾讯 chat相关云服务的支持 （api flavor，以及api key 的获得方式）
2. 对 百度 文心一言中 chat 相关云服务的支持 （同上）
3. 对 阿里 通义千问 中 chat 相关云服务的支持 （同上）
4. 调研以上几家对于文生图 相关 API的情况，为下一步文生图的支持做准备





## AOG 文档



### aog_doc_api_spec_cn: AOG API 中文文档

英文版（待完成）的转译，基于 reStructuredText



### aog_doc_dev_guide_cn: AOG使用文档中文版

建议以hello world的方式，通过对 sample app的讲解，向应用程序开发者展示如何使用 aog 来：

* 使用 aog_checker以及创建 `.aog`
* 程序内部调用 `aog_init()`以及 使用相关RESTful API来调用AI服务
* 测试
  * 运行自己的 sample app来自动安装所需服务并测试
  * 使用 aog manager 的 CLI来调整相关设置并测试

