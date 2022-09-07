﻿using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using NWO_RegionNode;
using System.Numerics;
using System.Text;

public class EchoServerHandler : ChannelHandlerAdapter
{

    public override void ChannelRead(IChannelHandlerContext context, object message)
    {
        var buffer = message as IByteBuffer;

        string rcv = buffer.ToString(Encoding.UTF8).Substring(2);
        Console.WriteLine("수신:" + buffer.GetByte(0) + "," + buffer.GetByte(1) + "," + buffer.GetByte(2) + "," + buffer.GetByte(3) + "," + buffer.GetByte(4) + "," + buffer.GetByte(5) + "," + buffer.GetByte(6) + "," + buffer.GetByte(7));
        
        //위치정보 정밀 동기화
        if(buffer.GetByte(2)==2&&buffer.GetByte(3)==1)
        {
            User Data;
            //유저 인덱스
            int userIndex = BitConverter.ToInt16(new byte[]{buffer.GetByte(3),buffer.GetByte(4)},0);


            //위치데이터 구성
            Vector3 UserPosition=new Vector3(
                new byte[]{buffer.GetByte(5),buffer.GetByte(6)}, 
                new byte[]{buffer.GetByte(7),buffer.GetByte(8)}, 
                new byte[]{buffer.GetByte(9),buffer.GetByte(10)});

            //속도데이터 구성
            int speed = BitConverter.ToInt16(new byte[]{buffer.GetByte(11),buffer.GetByte(12)});
            
            //각정보
            int rot = BitConverter.ToInt16(new byte[]{buffer.GetByte(13),buffer.GetByte(14)});

            //데이터 반영
            if (!Program.userTable.TryGetValue(userIndex, out Data))
            {
                Program.userTable.Add(userIndex, new User(context,0, UserPosition , speed, rot ) );

                Data = Program.userTable[userIndex];
            }
            else
            {
                Data.IChannel = context;
                Data.position = UserPosition;
                Data.speed = speed;
                Data.rot = rot;
            }
        }

        //속도데이터 구성

        //각정보 구성

        


        //context.WriteAsync("aaaa");

        /*StringBuilder std = new StringBuilder();

        std.Append("UD{");

        foreach (var userData in Program.userTable)
        {
            std.Append(userData.Key);
            std.Append("{");
            std.Append(userData.Value.position.X);
            std.Append(",");
            std.Append(userData.Value.position.Y);
            std.Append(",");
            std.Append(userData.Value.position.Z);
            std.Append(",");
            std.Append(userData.Value.speed);
            std.Append(",");
            std.Append(userData.Value.rot);
            std.Append("}");
        }
        std.Append("}");

        Thread.Sleep(20);

        IByteBuffer buf = Unpooled.CopiedBuffer(Encoding.UTF8.GetBytes(std.ToString()));*/

        //context.WriteAsync(buf);

        //Console.WriteLine("송신:"+buf.ToString(Encoding.UTF8));
    }

    public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

    public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
    {
        //Console.WriteLine("Exception: " + exception);
        context.CloseAsync();
    }

}