# Port Listener
This is a very basic TCP port listener that allows you to listen on a port range. If you run this program outside of firewall and run a port scanner inside a firewall pointing to the ip address where this program runs, the port scanner will be able you to tell which exactly ports are open on the firewall.

This code will run on Windows, but most importantly also on linux. DigitalOcean.com has all ports for their VMs open by default. So spin a new VM, copy *.cs in your (root) home folder and then run:
```
# apt-get update
# apt-get install mono-mcs -y
# mcs Program.cs Config.cs ListenerState.cs Options.cs -out:pln.exe
# ulimit -n 66000
# ./pln.exe 1 65535
```

Now you can use the VM ip address to determine open ports on your firewall Note that this is a dev utility, and is aimed to be minimal, no error handling. In case of a error stack trace is dumpled to console

```
Usage: pln.exe startPort [endPort] [/b=[VALUE]] [/c] [/s] [/t]
  -b[=VALUE]   display binding progress every VALUE ports. Default 100.
  -c           display incoming connections
  -s           suppress binding errors
  -t           display timestamp
```
