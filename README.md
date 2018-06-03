# TURN

A simple TCP port forwarding program, which is based on async IO. 

一个基于异步IO的简单TCP端口转发程序。

# Usage

exe [\<IP:Port\> \<TurnedIP:TurnedPort\>]...

## Example 1

exe 0.0.0.0:6666 192.168.1.1:10001

Any TCP connection to 0.0.0.0:6666 will be redirected to 192.168.1.1:10001.

所有到0.0.0.0:6666的TCP连接将会被转发到192.168.1.1:10001。

## Example 2

exe 0.0.0.0:6666 192.168.1.1:10001 0.0.0.0:7777 192.168.1.2:10002

Any TCP connection to 0.0.0.0:6666 will be redirected to 192.168.1.1:10001.

Any TCP connection to 0.0.0.0:7777 will be redirected to 192.168.1.2:10002.

所有到0.0.0.0:6666的TCP连接将会被转发到192.168.1.1:10001。

所有到0.0.0.0:7777的TCP连接将会被转发到192.168.1.2:10002。
