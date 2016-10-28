# Port Listener
This is a very basic TCP port listener that allows you to listen on a port range. 
If you run this program outside of a firewall and run a port scanner inside the firewall pointing to the ip address where this program runs, 
the port scanner will be able to tell you exactly which ports are open on the firewall.

This code will run on Windows, but most importantly also on linux. DigitalOcean.com has all ports for their VMs open by default. 
So spin up a new ubuntu VM and then run:
```
# apt-get update
# apt-get install git mono-mcs -y
# git clone https://github.com/AndrewSav/PortListener.git
# cd PortListener
# mcs CommandLine.cs Config.cs Connection.cs Hex.cs ListenerState.cs Options.cs Program.cs -out:pln.exe
# ulimit -n 66000
# ./pln.exe 1 65535 -c -t
```
Now you can use the VM ip address to determine open ports on your firewall. 
Note that this is a dev utility, and is aimed to be minimal, no error handling. 
In case of a error, stack trace is dumpled to console.
```
Usage: pln.exe startPort [endPort] [/b=[VALUE]] [/c] [/s] [/t] [/d=[VALUE]] [/w=VALUE] [i=a.b.c.d,a.b.c.d]
  -b[=VALUE]   display binding progress every VALUE ports. Default 100.
  -c           display incoming connections
  -s           suppress binding errors
  -t           display timestamp
  -d[=VALUE]   dump first VALUE bytes of incoming data. Default 255. Only works when /c is specified
  -w=VALUE     incoming data wait time. Default - do not wait. Only works when /d is specified
  -i=VALUE     Only bind on specific interfaces (ip addresses)
```
