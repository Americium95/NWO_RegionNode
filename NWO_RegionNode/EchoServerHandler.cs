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
        //Console.WriteLine("수신:" + buffer.GetByte(0) + "," + buffer.GetByte(1) + "," + buffer.GetByte(2) + "," + buffer.GetByte(3) + "," + buffer.GetByte(4) + "," + buffer.GetByte(5) + "," + buffer.GetByte(6) + "," + buffer.GetByte(7));
        
        //위치정보 정밀 동기화
        if(buffer.GetByte(2)==2&&buffer.GetByte(3)==1)
        {
            User Data;
            //유저 인덱스
            int userIndex = BitConverter.ToInt16(new byte[]{buffer.GetByte(4),buffer.GetByte(5)},0);

            //타일 위치데이터 구성
            Vector2 tilePosition=new Vector2(
                BitConverter.ToInt16(new byte[]{buffer.GetByte(6),buffer.GetByte(7)}),
                BitConverter.ToInt16(new byte[]{buffer.GetByte(8),buffer.GetByte(9)}));

            //위치데이터 구성
            Vector3 UserPosition=new Vector3(
                BitConverter.ToInt16(new byte[]{buffer.GetByte(10),buffer.GetByte(11)}), 
                BitConverter.ToInt16(new byte[]{buffer.GetByte(12),buffer.GetByte(13)}), 
                BitConverter.ToInt16(new byte[]{buffer.GetByte(14),buffer.GetByte(15)}));
            Console.WriteLine(UserPosition);
            //속도데이터 구성
            int speed = BitConverter.ToInt16(new byte[]{buffer.GetByte(16),buffer.GetByte(17)});
            
            //각정보
            byte rot = buffer.GetByte(18);

            //데이터 반영
            if (!Program.userTable.TryGetValue(userIndex, out Data))
            {
                Program.userTable.Add(userIndex, new User(context,0, tilePosition, UserPosition , speed, rot ) );

                Data = Program.userTable[userIndex];

                Data.position = UserPosition;
                Data.speed = speed;
                Data.rot = rot;
            }
            else
            {
                Data.IChannel = context;
                Data.position = UserPosition;
                Data.speed = speed;
                Data.rot = rot;
            }
        }

        //위치정보 근사 동기화
        if(buffer.GetByte(2)==2&&buffer.GetByte(3)==2)
        {
            User Data;
            //유저 인덱스
            int userIndex = BitConverter.ToInt16(new byte[]{buffer.GetByte(4),buffer.GetByte(5)},0);



            //속도데이터 구성
            int speed = BitConverter.ToInt16(new byte[]{buffer.GetByte(6),buffer.GetByte(7)});
            
            //각정보
            byte rot = buffer.GetByte(8);

            //데이터 반영
            if (Program.userTable.TryGetValue(userIndex, out Data))
            {
                Data.IChannel = context;
                Data.position = Data.position+new Vector3(MathF.Sin(rot),0,MathF.Cos(rot))*speed/10;
                Data.speed = speed;
                Data.rot = rot;
            }
        }


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