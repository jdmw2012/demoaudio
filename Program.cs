using FFmpeg.AutoGen;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace ConsoleApp1
{
    class Program
    {

        static CaptureState _state = CaptureState.PREPARED;
        static int count = 0;

        unsafe static void Main(string[] args)
        {


            ffmpeg.RootPath = @"D:\cshapdemo\ConsoleApp1\ffmpeg";
            var file1 = @"D:\cshapdemo\ConsoleApp1\会不会.mp3";
            var file2 = @"D:\cshapdemo\ConsoleApp1\无人之岛-任然.mp3";
            Openfile openfile1 = new Openfile();
            Openfile openfile2 = new Openfile();
            openfile1.open(file1);
            openfile2.open(file2);

            Openfile outfile = new Openfile();

            outfile.OpenFileOutput(@"D:\cshapdemo\ConsoleApp1\3.mp3");
            Console.WriteLine("eee");
            AVAudioFifo* aVAudioFifo1 = ffmpeg.av_audio_fifo_alloc(
                openfile1._pCodecContext->sample_fmt,
                openfile1._pCodecContext->channels,
                30 * openfile1._pCodecContext->frame_size);

            AVAudioFifo* aVAudioFifo2 = ffmpeg.av_audio_fifo_alloc(
                openfile2._pCodecContext->sample_fmt,
                openfile2._pCodecContext->channels,
                30 * openfile2._pCodecContext->frame_size);
            //打开输入输出文件
            Console.WriteLine("aVAudioFifo2 ");
            // readFile(openfile1, aVAudioFifo1);
            //配置过滤器
            AVFilterGraph* _filter_graph = null;
            AVFilterContext* _filter_ctx_src_spk = null;
            AVFilterContext* _filter_ctx_src_mic = null;
            AVFilterContext* _filter_ctx_sink = null;
            string filter_desc = "[in0][in1]amix=inputs=2[out]";
            //  InitFilter(_filter_graph, filter_desc, _filter_ctx_src_spk, _filter_ctx_src_mic, _filter_ctx_sink, openfile1, openfile2, outfile);

            MyFilter myFilter = new MyFilter();
            myFilter.InitFilter(_filter_graph, filter_desc, _filter_ctx_src_spk, _filter_ctx_src_mic, _filter_ctx_sink, openfile1, openfile2, outfile);


            //Thread thread1 = new Thread(start: new ThreadStart(new My(openfile1, aVAudioFifo1).C));

            // Thread thread2 = new Thread(start: new ThreadStart(new My(openfile2, aVAudioFifo2).C));
            readFile(openfile1, aVAudioFifo1);
            readFile(openfile2, aVAudioFifo2);
            if (count == 2)
            {
                Console.WriteLine("2222222222222222222");

            }
            int tmpFifoFailed = 0;
            int frame_count = 0;
            while (true)
            {

                AVFrame* pFrame_spk = ffmpeg.av_frame_alloc();
                AVFrame* pFrame_mic = ffmpeg.av_frame_alloc();

                AVPacket packet_out;
                int got_packet_ptr = 0;

                int fifo_spk_size = ffmpeg.av_audio_fifo_size(aVAudioFifo1);
                int fifo_mic_size = ffmpeg.av_audio_fifo_size(aVAudioFifo2);
                int frame_spk_min_size = openfile1._pFormatContext->streams[openfile1._streamIndex]->codecpar->frame_size;
                int frame_mic_min_size = openfile2._pFormatContext->streams[openfile2._streamIndex]->codecpar->frame_size;

                if (fifo_spk_size >= frame_spk_min_size && fifo_mic_size >= frame_mic_min_size)
                {

                    tmpFifoFailed = 0;

                    pFrame_spk->nb_samples = frame_spk_min_size;
                    pFrame_spk->channel_layout = openfile1._pFormatContext->streams[openfile1._streamIndex]->codecpar->channel_layout;
                    pFrame_spk->format = (int)openfile1._pFormatContext->streams[openfile1._streamIndex]->codec->sample_fmt;
                    pFrame_spk->sample_rate = openfile1._pFormatContext->streams[openfile1._streamIndex]->codecpar->sample_rate;
                    int ret = ffmpeg.av_frame_get_buffer(pFrame_spk, 0);
                    if (ret < 0)
                    {
                        Console.WriteLine("av_frame_get_buffer pFrame_spk failed");
                    }
                    pFrame_mic->nb_samples = frame_mic_min_size;
                    pFrame_mic->channel_layout = openfile2._pFormatContext->streams[openfile2._streamIndex]->codecpar->channel_layout;
                    pFrame_mic->format = (int)openfile2._pFormatContext->streams[openfile2._streamIndex]->codec->sample_fmt;
                    pFrame_mic->sample_rate = openfile2._pFormatContext->streams[openfile2._streamIndex]->codecpar->sample_rate;
                    ret = ffmpeg.av_frame_get_buffer(pFrame_mic, 0);
                    if (ret < 0)
                    {
                        Console.WriteLine("av_frame_get_buffer pFrame_mic failed");
                    }

                    int nSizeOfPerson = Marshal.SizeOf(pFrame_spk->data);                 //定义指针长度
                    IntPtr spkX = Marshal.AllocHGlobal(nSizeOfPerson);        //定义指针
                    Marshal.StructureToPtr(pFrame_spk->data, spkX, true);                //将结构体person转为personX指针
                    ret = ffmpeg.av_audio_fifo_read(aVAudioFifo1, (void**)spkX, frame_spk_min_size);//读取数据1
                    Thread.Sleep(1000);


                    int nSizeOfPerson2 = Marshal.SizeOf(pFrame_mic->data);                 //定义指针长度
                    IntPtr spkX2 = Marshal.AllocHGlobal(nSizeOfPerson2);        //定义指针
                    Marshal.StructureToPtr(pFrame_mic->data, spkX2, true);                //将结构体person转为personX指针
                    ret = ffmpeg.av_audio_fifo_read(aVAudioFifo2, (void**)spkX2, frame_mic_min_size);//读取数据1

                    //  Thread thread1 = new Thread(start:new ThreadStart(new My2(pFrame_mic, aVAudioFifo2).C));




                    pFrame_spk->pts = ffmpeg.av_frame_get_best_effort_timestamp(pFrame_spk);
                    pFrame_mic->pts = ffmpeg.av_frame_get_best_effort_timestamp(pFrame_mic);

                    _filter_ctx_src_spk =myFilter._filter_ctx_src_spk;
                     ret = ffmpeg.av_buffersrc_add_frame(_filter_ctx_src_spk, pFrame_spk);//交给filter
                    if (ret < 0)
                    {
                        Console.WriteLine("Mixer: failed to call av_buffersrc_add_frame (speaker)\n");
                        break;
                    }

                    ret = ffmpeg.av_buffersrc_add_frame(_filter_ctx_src_mic, pFrame_mic);
                    if (ret < 0)
                    {
                        Console.WriteLine("Mixer: failed to call av_buffersrc_add_frame (mic)\n");
                        break;
                    }



                    //取出滤镜混合后的样本数据
                    //对数据进行编码后写入IO
                    //关闭资源关闭io
                    while (true)
                    {

                        AVFrame* pFrame_out = ffmpeg.av_frame_alloc();
                        ret = ffmpeg.av_buffersink_get_frame_flags(_filter_ctx_sink, pFrame_out, 0);
                        if (ret < 0)
                        {
                            Console.WriteLine("Mixer: failed to call av_buffersink_get_frame_flags\n");
                            break;
                        }
                        if (pFrame_out->data[0] != null)
                        {
                            ffmpeg.av_init_packet(&packet_out);
                            packet_out.data = null;
                            packet_out.size = 0;

                            ret = ffmpeg.avcodec_encode_audio2(outfile._pFormatContext->streams[outfile._streamIndex]->codec, &packet_out, pFrame_out, &got_packet_ptr);
                            if (ret < 0)
                            {
                                Console.WriteLine("Mixer: failed to call avcodec_decode_audio4\n");
                                break;
                            }
                            if (got_packet_ptr > 0)
                            {
                                packet_out.stream_index = outfile._streamIndex;
                                packet_out.pts = frame_count * outfile._pFormatContext->streams[outfile._streamIndex]->codec->frame_size;
                                packet_out.dts = packet_out.pts;
                                packet_out.duration = outfile._pFormatContext->streams[outfile._streamIndex]->codec->frame_size;

                                packet_out.pts = ffmpeg.av_rescale_q_rnd(packet_out.pts,
                                    outfile._pFormatContext->streams[outfile._streamIndex]->codec->time_base,
                                    outfile._pFormatContext->streams[outfile._streamIndex]->time_base,
                                    (AVRounding)(1 | 8192));

                                packet_out.dts = packet_out.pts;

                                packet_out.duration = ffmpeg.av_rescale_q_rnd(packet_out.duration,
                                     outfile._pFormatContext->streams[outfile._streamIndex]->codec->time_base,
                                     outfile._pFormatContext->streams[outfile._streamIndex]->time_base,
                                    (AVRounding)(1 | 8192));

                                frame_count++;

                                ret = ffmpeg.av_interleaved_write_frame(outfile._pFormatContext, &packet_out);
                                if (ret < 0)
                                {
                                    Console.WriteLine("Mixer: failed to call av_interleaved_write_frame\n");
                                }
                                Console.WriteLine("Mixer: write frame to file\n");
                            }
                            ffmpeg.av_free_packet(&packet_out);
                        }
                        ffmpeg.av_frame_free(&pFrame_out);
                    }
                }
                else
                {

                    //===========================================================================
                    tmpFifoFailed++;

                    if (tmpFifoFailed > 300)
                    {


                        break;
                    }
                    ffmpeg.av_frame_free(&pFrame_spk);
                    ffmpeg.av_frame_free(&pFrame_mic);


                }
            }
            ffmpeg.av_write_trailer(outfile._pFormatContext);
        }
        unsafe class My
        {

            public Openfile openfile;
            public AVAudioFifo* aVAudioFifo;

            public My(Openfile openfile, AVAudioFifo* aVAudioFifo)
            {
                this.openfile = openfile;
                this.aVAudioFifo = aVAudioFifo;
            }

            public void C()
            {
                readFile(openfile, aVAudioFifo);
            }
        }
        enum CaptureState
        {
            PREPARED,
            RUNNING,
            STOPPED,
            FINISHED
        };

        unsafe class MyFilter
        {
            public AVFilterGraph* _filter_graph = null;
            public AVFilterContext* _filter_ctx_src_spk = null;
            public AVFilterContext* _filter_ctx_src_mic = null;
            public AVFilterContext* _filter_ctx_sink = null;
            unsafe public void InitFilter(AVFilterGraph* _filter_graph,
                              String filter_desc,
                              AVFilterContext* _filter_ctx_src_spk,
                              AVFilterContext* _filter_ctx_src_mic,
                              AVFilterContext* _filter_ctx_sink,
                              Openfile openfile1,
                              Openfile openfile2,
                              Openfile outfile)
            {
                AVFilter* filter_src_spk = ffmpeg.avfilter_get_by_name("abuffer");
                AVFilter* filter_src_mic = ffmpeg.avfilter_get_by_name("abuffer");
                AVFilter* filter_sink = ffmpeg.avfilter_get_by_name("abuffersink");

                AVFilterInOut* filter_output_spk = ffmpeg.avfilter_inout_alloc();
                AVFilterInOut* filter_output_mic = ffmpeg.avfilter_inout_alloc();
                AVFilterInOut* filter_input = ffmpeg.avfilter_inout_alloc();

                _filter_graph = ffmpeg.avfilter_graph_alloc();
                string args_spk = "time_base=1/44100:sample_rate=44100:sample_fmt=7:channel_layout=3";


                // 5.1 创建输出滤镜的上下文
                //_filter_ctx_sink = ffmpeg.avfilter_graph_alloc_filter(_filter_graph, filter_sink, "sink");

                int ret = ffmpeg.avfilter_graph_create_filter(&_filter_ctx_src_spk, filter_src_spk, "in0", args_spk, null, _filter_graph);
                ret = ffmpeg.avfilter_graph_create_filter(&_filter_ctx_src_mic, filter_src_mic, "in1", args_spk, null, _filter_graph);
                ret = ffmpeg.avfilter_graph_create_filter(&_filter_ctx_sink, filter_sink, "out", null, null, _filter_graph);

                AVCodecContext* encodec_ctx = outfile._pCodecContext;

                //过滤器上下文输出参数配置

                //ret = ffmpeg.av_opt_set_bin(_filter_ctx_sink, "sample_rates", (byte*)outfile._pCodecContext->sample_rate, (int)8, 1);
                //if (ret < 0)
                //{
                //    Console.WriteLine("Filter: failed to call av_opt_set_bin -- sample_rates\n");
                //}

                ret = ffmpeg.av_opt_set_bin(_filter_ctx_sink, "sample_fmts", (byte*)&encodec_ctx->sample_fmt, (int)4, 1);
                if (ret < 0)
                {
                    Console.WriteLine("Filter: failed to call av_opt_set_bin -- sample_fmts\n");
                }
                ret = ffmpeg.av_opt_set_bin(_filter_ctx_sink, "channel_layouts", (byte*)&encodec_ctx->channel_layout, (int)8, 1);

                if (ret < 0)
                {
                    Console.WriteLine("Filter: failed to call av_opt_set_bin -- channel_layouts\n");
                }





                filter_output_spk->name = ffmpeg.av_strdup("in0");
                filter_output_spk->filter_ctx = _filter_ctx_src_spk;
                filter_output_spk->pad_idx = 0;
                filter_output_spk->next = filter_output_mic;

                filter_output_mic->name = ffmpeg.av_strdup("in1");
                filter_output_mic->filter_ctx = _filter_ctx_src_mic;
                filter_output_mic->pad_idx = 0;
                filter_output_mic->next = null;

                filter_input->name = ffmpeg.av_strdup("out");
                filter_input->filter_ctx = _filter_ctx_sink;
                filter_input->pad_idx = 0;
                filter_input->next = null;

                AVFilterInOut*[] filter_outputs = new AVFilterInOut*[2];
                filter_outputs[0] = filter_output_spk;
                filter_outputs[1] = filter_output_mic;

                fixed (AVFilterInOut** outs = &filter_outputs[0])
                {

                    ret = ffmpeg.avfilter_graph_parse_ptr(_filter_graph, filter_desc, &filter_input, outs, null);
                    ret = ffmpeg.avfilter_graph_config(_filter_graph, null);
                    if (ret < 0)
                    {
                        Console.WriteLine("Filter: failed to call avfilter_graph_config -- avfilter_graph_config\n");
                    }
                    byte* ff = ffmpeg.avfilter_graph_dump(_filter_graph, null);



                    Console.WriteLine("Filter: failed to call avfilter_graph_dump -- avfilter_graph_dump\n" + *ff);
                }

                this._filter_ctx_src_spk = _filter_ctx_src_spk;
            }


        }

        //unsafe static void InitFilter(AVFilterGraph* _filter_graph,
        //                         String filter_desc,
        //                         AVFilterContext* _filter_ctx_src_spk,
        //                         AVFilterContext* _filter_ctx_src_mic,
        //                         AVFilterContext* _filter_ctx_sink,
        //                         Openfile openfile1,
        //                         Openfile openfile2,
        //                         Openfile outfile)
        //{
        //    AVFilter* filter_src_spk = ffmpeg.avfilter_get_by_name("abuffer");
        //    AVFilter* filter_src_mic = ffmpeg.avfilter_get_by_name("abuffer");
        //    AVFilter* filter_sink = ffmpeg.avfilter_get_by_name("abuffersink");

        //    AVFilterInOut* filter_output_spk = ffmpeg.avfilter_inout_alloc();
        //    AVFilterInOut* filter_output_mic = ffmpeg.avfilter_inout_alloc();
        //    AVFilterInOut* filter_input = ffmpeg.avfilter_inout_alloc();

        //    _filter_graph = ffmpeg.avfilter_graph_alloc();
        //    string args_spk = "time_base=1/44100:sample_rate=44100:sample_fmt=7:channel_layout=3";


        //    // 5.1 创建输出滤镜的上下文
        //    //_filter_ctx_sink = ffmpeg.avfilter_graph_alloc_filter(_filter_graph, filter_sink, "sink");

        //    int ret = ffmpeg.avfilter_graph_create_filter(&_filter_ctx_src_spk, filter_src_spk, "in0", args_spk, null, _filter_graph);
        //    ret = ffmpeg.avfilter_graph_create_filter(&_filter_ctx_src_mic, filter_src_mic, "in1", args_spk, null, _filter_graph);
        //    ret = ffmpeg.avfilter_graph_create_filter(&_filter_ctx_sink, filter_sink, "out", null, null, _filter_graph);

        //    AVCodecContext* encodec_ctx = outfile._pCodecContext;

        //    //过滤器上下文输出参数配置

        //    //ret = ffmpeg.av_opt_set_bin(_filter_ctx_sink, "sample_rates", (byte*)outfile._pCodecContext->sample_rate, (int)8, 1);
        //    //if (ret < 0)
        //    //{
        //    //    Console.WriteLine("Filter: failed to call av_opt_set_bin -- sample_rates\n");
        //    //}

        //    ret = ffmpeg.av_opt_set_bin(_filter_ctx_sink, "sample_fmts", (byte*)&encodec_ctx->sample_fmt, (int)4, 1);
        //    if (ret < 0)
        //    {
        //        Console.WriteLine("Filter: failed to call av_opt_set_bin -- sample_fmts\n");
        //    }
        //    ret = ffmpeg.av_opt_set_bin(_filter_ctx_sink, "channel_layouts", (byte*)&encodec_ctx->channel_layout, (int)8, 1);

        //    if (ret < 0)
        //    {
        //        Console.WriteLine("Filter: failed to call av_opt_set_bin -- channel_layouts\n");
        //    }





        //    filter_output_spk->name = ffmpeg.av_strdup("in0");
        //    filter_output_spk->filter_ctx = _filter_ctx_src_spk;
        //    filter_output_spk->pad_idx = 0;
        //    filter_output_spk->next = filter_output_mic;

        //    filter_output_mic->name = ffmpeg.av_strdup("in1");
        //    filter_output_mic->filter_ctx = _filter_ctx_src_mic;
        //    filter_output_mic->pad_idx = 0;
        //    filter_output_mic->next = null;

        //    filter_input->name = ffmpeg.av_strdup("out");
        //    filter_input->filter_ctx = _filter_ctx_sink;
        //    filter_input->pad_idx = 0;
        //    filter_input->next = null;

        //    AVFilterInOut*[] filter_outputs = new AVFilterInOut*[2];
        //    filter_outputs[0] = filter_output_spk;
        //    filter_outputs[1] = filter_output_mic;

        //    fixed (AVFilterInOut** outs = &filter_outputs[0])
        //    {

        //        ret = ffmpeg.avfilter_graph_parse_ptr(_filter_graph, filter_desc, &filter_input, outs, null);
        //        ret = ffmpeg.avfilter_graph_config(_filter_graph, null);
        //        if (ret < 0)
        //        {
        //            Console.WriteLine("Filter: failed to call avfilter_graph_config -- avfilter_graph_config\n");
        //        }
        //        byte* ff = ffmpeg.avfilter_graph_dump(_filter_graph, null);



        //        Console.WriteLine("Filter: failed to call avfilter_graph_dump -- avfilter_graph_dump\n" + *ff);
        //    }

        //    _filter_ctx_src_spk = _filter_ctx_src_spk;
        //}


        unsafe class My2
        {
            public int frame_mic_min_size;
            public AVFrame* pFrame;
            public AVAudioFifo* aVAudioFifo;

            public My2(AVFrame* pFrame, AVAudioFifo* aVAudioFifo)
            {
                this.pFrame = pFrame;
                this.aVAudioFifo = aVAudioFifo;
            }

            public void C()
            {

                int nSizeOfPerson2 = Marshal.SizeOf(pFrame->data);                 //定义指针长度
                IntPtr micX = Marshal.AllocHGlobal(nSizeOfPerson2);        //定义指针
                Marshal.StructureToPtr(pFrame->data, micX, true);                //将结构体person转为personX指针
                ffmpeg.av_audio_fifo_read(aVAudioFifo, (void**)micX, frame_mic_min_size);//读取数据2
            }
        }
        public unsafe static void readFile(Openfile openFlie, AVAudioFifo* aVAudioFifo1)//解码
        {
            AVFrame* pFrame = ffmpeg.av_frame_alloc();//接受解码后的数据
            AVPacket packet;
            ffmpeg.av_init_packet(&packet);




            //  while (_state == CaptureState.RUNNING)
            while (true)
            {

                Console.WriteLine("count" + count);
                packet.data = null;
                packet.size = 0;

                if (ffmpeg.av_read_frame(openFlie._pFormatContext, &packet) < 0)
                {
                    Console.WriteLine("嘟嘟去读完");
                    count++;
                    ffmpeg.av_packet_unref(&packet);
                    break;
                }
                //if (packet.stream_index == openFlie._streamIndex)
                //{
                Console.WriteLine(packet.size);
                int ret = ffmpeg.avcodec_send_packet(openFlie._pCodecContext, &packet);//发送解码数据

                if (ret < 0)
                {
                    Console.WriteLine("avcodec_send_packet failed");
                    break;
                }

                ffmpeg.avcodec_receive_frame(openFlie._pCodecContext, pFrame);// 接受解码后的数据


                int fifo_mic_space = ffmpeg.av_audio_fifo_space(aVAudioFifo1);
                while (fifo_mic_space < pFrame->nb_samples && _state == CaptureState.RUNNING)//样本数（nb_samples解码后一帧大小）
                {

                    Console.WriteLine("_fifo_ full !\n");
                    fifo_mic_space = ffmpeg.av_audio_fifo_space(aVAudioFifo1);
                }

                if (fifo_mic_space >= pFrame->nb_samples)
                {

                    int temp = ffmpeg.av_audio_fifo_space(aVAudioFifo1);
                    int temp2 = pFrame->nb_samples;
                    int nSizeOfPerson = Marshal.SizeOf(pFrame->data);                 //定义指针长度
                    IntPtr personX = Marshal.AllocHGlobal(nSizeOfPerson);        //定义指针
                    Marshal.StructureToPtr(pFrame->data, personX, true);                //将结构体person转为personX指针

                    int nWritten = ffmpeg.av_audio_fifo_write(aVAudioFifo1, (void**)personX, pFrame->nb_samples);

                }
                ffmpeg.av_packet_unref(&packet);
                //   }
            }
            ffmpeg.av_frame_free(&pFrame);




        }
    }
}
