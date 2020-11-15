using FFmpeg.AutoGen;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp1
{
    sealed unsafe class Openfile : IDisposable
    {
        public AVCodecContext* _pCodecContext;
        public AVFormatContext* _pFormatContext;
        public int _streamIndex;
        public AVCodec* _pCodec;

       

   

        public void Dispose()
        {
            ffmpeg.avcodec_close(_pCodecContext);
            ffmpeg.av_free(_pCodecContext);
            ffmpeg.av_free(_pCodec);
        }
        //@"D:\cshapdemo\ConsoleApp1\会不会.mp3"
        public void open(String url)
        {

            //var codecId = AVCodecID.AV_CODEC_ID_MP3;
            //_pCodecContext = ffmpeg.avcodec_alloc_context3(_pCodec);
            //        _pCodec = ffmpeg.avcodec_find_encoder(codecId);
            //_pCodecContext = ffmpeg.avcodec_alloc_context3(_pCodec);
            //_pCodecContext->time_base = new AVRational { num = 1, den = fps };
            //ret = ffmpeg.avcodec_open2(_pCodecContext, _pCodec, null);

            int ret = 0;
            _pFormatContext = ffmpeg.avformat_alloc_context();
            var pFormatContext = _pFormatContext;
            ret = ffmpeg.avformat_open_input(&pFormatContext, url, null, null);
            ret = ffmpeg.avformat_find_stream_info(pFormatContext, null);
            for (int i = 0; i < pFormatContext->nb_streams; i++)
            {
                if (pFormatContext->streams[i]->codecpar->codec_type == AVMediaType.AVMEDIA_TYPE_AUDIO)
                {
                    _streamIndex = i;
                    break;
                }
            }
            _pCodecContext = pFormatContext->streams[_streamIndex]->codec;
            AVCodec* codec = ffmpeg.avcodec_find_decoder(_pCodecContext->codec_id);
            ret = ffmpeg.avcodec_open2(_pCodecContext, codec, null);//初始化编码器
            ffmpeg.av_dump_format(pFormatContext, _streamIndex, url, 0);



        }
        //打开输出文件IO
        public void OpenFileOutput(String fileName)
        {
            int ret = 0;
            //fixed (AVFormatContext** _pTr = &_pFormatContext)
            //{

            //    ret = ffmpeg.avformat_alloc_output_context2(_pTr, null, null, fileName);
            //    AVStream* stream_a = null;
            //    stream_a = ffmpeg.avformat_new_stream(_pFormatContext, null);

            //    stream_a->codecpar->codec_type = FFmpeg.AutoGen.AVMediaType.AVMEDIA_TYPE_AUDIO;

            //    AVCodec* codec_mp3 = ffmpeg.avcodec_find_encoder(AVCodecID.AV_CODEC_ID_MP3);
            //    if (codec_mp3 == null) {
            //        Console.WriteLine("avcodec_find_encoder failed");
            //        return;
            //    }
            //    stream_a->codec->codec = codec_mp3;
            //    stream_a->codecpar->sample_rate = 16000;
            //    stream_a->codecpar->channels = 1;
            //    stream_a->codecpar->channel_layout = (ulong)ffmpeg.av_get_default_channel_layout(1);
            //    stream_a->codec->sample_rate = 16000;
            //    stream_a->codec->channels = 1;
            //    stream_a->codec->channel_layout = (ulong)ffmpeg.av_get_default_channel_layout(1);
            //    stream_a->codec->sample_fmt = codec_mp3->sample_fmts[0];
            //    stream_a->codecpar->bit_rate = 16000;
            //    stream_a->codec->bit_rate = 16000;
            //    stream_a->codec->time_base.num = 1;
            //    stream_a->codec->time_base.den = stream_a->codecpar->sample_rate;
            //    stream_a->codecpar->codec_tag = 0;
            //    stream_a->codec->codec_tag = 0;

            //    stream_a->codec->flags = (1 << 22);
            //    Console.WriteLine(1 << 22);
            //    if (ffmpeg.avcodec_open2(stream_a->codec, stream_a->codec->codec, null) < 0)
            //    {
            //        Console.WriteLine("Mixer: failed to call avcodec_open2\n");

            //    }
            //    if (ffmpeg.avio_open(&_pFormatContext->pb, fileName, 2) < 0)
            //    {
            //        Console.WriteLine("Mixer: failed to call avio_open\n");

            //    }

            //    ffmpeg.av_dump_format(_pFormatContext, 0, fileName, 1);

            //    if (ffmpeg.avformat_write_header(_pFormatContext, null) < 0)
            //    {
            //        Console.WriteLine("Mixer: failed to call avformat_write_header\n");
            //    }
            //}
            fixed (AVFormatContext** _pTr = &_pFormatContext)
            {



                _pCodec = ffmpeg.avcodec_find_encoder(AVCodecID.AV_CODEC_ID_MP3);
                if (_pCodec == null)
                {
                    Console.WriteLine("avcodec_find_encoder failed");
                    return;
                }
                 _pCodecContext = ffmpeg.avcodec_alloc_context3(_pCodec);

                if (_pCodecContext == null)
                {
                    Console.WriteLine("avcodec_alloc_context3 out failed");
                    return;
                }


                //AV_CH_LAYOUT_STEREO=2
                _pCodecContext->sample_rate = 44100;
                _pCodecContext->time_base.den = 44100;
                _pCodecContext->time_base.num = 1;
                _pCodecContext->bit_rate = 320000;
                _pCodecContext->channels = 2;
                _pCodecContext->channel_layout = 3;
                _pCodecContext->sample_fmt = FFmpeg.AutoGen.AVSampleFormat.AV_SAMPLE_FMT_S32P;//4个字节
                _pCodecContext->flags |= (1 << 22);

                ret = ffmpeg.avcodec_open2(_pCodecContext, _pCodec, null);
                if (ret < 0)
                {
                    Console.WriteLine("===============Mixer: failed to call avcodec_open2\n");
                }

                //输出上下文
                
                ret = ffmpeg.avformat_alloc_output_context2(_pTr, null, null, fileName);
                if (ret < 0)
                {
                    Console.WriteLine("Mixer: failed to call avformat_alloc_output_context2\n");
                }
                //添加一个音频流
                AVStream* stream_audio = null;
                stream_audio = ffmpeg.avformat_new_stream(_pFormatContext, null);
                stream_audio->codecpar->codec_tag = 0;
                ffmpeg.avcodec_parameters_from_context(stream_audio->codecpar, _pCodecContext);

                //打开输出文件的IO流
                ret = ffmpeg.avio_open(&_pFormatContext->pb, fileName, 2);
                if (ret < 0)
                {
                    Console.WriteLine("Mixer: failed to call avio_open\n");
                }
                //写入头部信息
                ffmpeg.avformat_write_header(_pFormatContext, null);
                ffmpeg.av_dump_format(_pFormatContext, 0, fileName, 1);
            }
            //写入缓存



        }

    }
}
