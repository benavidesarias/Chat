using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.IO;
using System.IO.Pipes;
using System.Threading;

namespace Chat
{
    class Servidor
    {
        IPEndPoint localEndPoint;
        TcpListener listener;
        int puerto;
        NetworkStream stream;
        
        
        public Servidor(int puerto)
        {
           

            this.puerto = puerto;
            String strip = GetLocalIPv4();
            IPAddress ip = IPAddress.Parse(strip);

            Console.WriteLine("Iniciando servidor en ip:{0} puerto:{1}",strip,puerto);
            
            localEndPoint = new IPEndPoint(ip, puerto);
            listener = new TcpListener(localEndPoint);
            listener.Start();
            Console.WriteLine("Esperando conexiones ...");

            Thread work = new Thread(new ThreadStart(refresh));
            Thread teclado = new Thread(new ThreadStart(leerTeclado));
            Thread conexion = new Thread(new ThreadStart(clientes));

            work.Start();
            teclado.Start();
            conexion.Start();
           
        }

        public void clientes()
        {
            // Bind the socket to the local endpoint and listen for incoming connections.
            while (true)
            {

                try
                {

                    TcpClient client = listener.AcceptTcpClient();
                    String ipclient = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                    String portclient = ((IPEndPoint)client.Client.RemoteEndPoint).Port.ToString();
                    Console.WriteLine("Conexion desde ip:{0} puerto:{1}", ipclient, portclient);
                    Console.WriteLine();

                    stream = client.GetStream();

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

            }
        }

        public void leerTeclado()
        {
            while (true)
                        {
                                String mensaje = Console.ReadLine();
                                sendMessage(mensaje);                
                            
                        }
        }

        public void sendMessage(String message)
        {
            try
            {
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

                // Send the message to the connected TcpServer. 
                stream.Write(data, 0, data.Length);
                //stream.Flush();
            }
            catch (Exception e) { }
        }

        public String readMessage()
        {
            String responseData = String.Empty;

            try
            {
                Byte[] data = new Byte[256];

                // String to store the response ASCII representation.


                // Read the first batch of the TcpServer response bytes.
                Int32 bytes = stream.Read(data, 0, data.Length);
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                
                //stream.Flush();
            }
            catch (Exception e) { }

            return responseData;
        }

        public void refresh()
        {
            while (true)
            {
                try
                {
                    String message = readMessage();

                    if (!message.Equals(String.Empty))
                    {
                        Console.WriteLine(message);
                        
                    }
                }
                catch (Exception e) { }
            }

        }

        internal static string GetLocalIPv4()
        {
            string output = "";
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
                if ( item.OperationalStatus == OperationalStatus.Up)
                {
                    IPInterfaceProperties adapterProperties = item.GetIPProperties();

                    if (adapterProperties.GatewayAddresses.FirstOrDefault() != null)
                    {
                        foreach (UnicastIPAddressInformation ip in adapterProperties.UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            {
                                output = ip.Address.ToString();
                            }
                        }
                    }
                }
            }

            return output;
        }
    }

    class Cliente
    {
        NetworkStream stream;
        int bandera = 0; //no se ha conectado

        public Cliente(String strip,String strport)
        {

            Console.WriteLine("Conectando a ip {0} puerto {1}", strip,strport);
            int port = int.Parse(strport);

            try
            {
                TcpClient client = new TcpClient(strip, port);
                stream = client.GetStream();
                Console.WriteLine("ok Empieza a escribir");
                bandera = 1;
                Thread work = new Thread(refresh);
                
                work.Start();

                while (true)
                {
                    String message = Console.ReadLine();
                    sendMessage(message);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error en la conexion");
            }
            
        }



        public void sendMessage(String message)
        {
            try{
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

                // Send the message to the connected TcpServer. 
                stream.Write(data, 0, data.Length);
                stream.Flush();
            
            }
            catch (Exception e) { }
        }

        public String readMessage()
        {
            String responseData = String.Empty;

            try
            {

                Byte[] data = new Byte[256];

                // String to store the response ASCII representation.
                

                // Read the first batch of the TcpServer response bytes.
                Int32 bytes = stream.Read(data, 0, data.Length);
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
 
                stream.Flush();

            }catch (Exception e) {
                if (bandera == 1)
                {
                    Console.WriteLine("Error en la conexion");
                    Environment.Exit(0);
                }
            }

            return responseData;
        }

        public void refresh()
        {
            while (true)
            {
                try{
                    String message = readMessage();
                    if(!message.Equals(String.Empty))
                        Console.WriteLine(message);
                }
                catch (Exception e)
                {
                   
                }
            }

        }
    }

    class Program
    {
        

        static void Main(string[] args)
        {
            //chat -s -p 8080
            //chat 10.101.150.241 8080
            //chat -m ipcliente puerto
            

            if (args.Length < 2 || args.Length > 3)
            {

                Console.WriteLine("ADSI - Instructor Andres Benavides, Sintaxis");
                Console.WriteLine("Modo Servidor: chat -s -p puerto");
                Console.WriteLine("Modo Cliente:  chat ip puerto");
                return;
            }

            if (args[0] == "-s")
            {
                
                Servidor servidor = new Servidor(int.Parse(args[2]));

            } 
            else
            {
                Cliente cliente = new Cliente(args[0], args[1]);
            }

            //Console.ReadKey();

        }
    }
}
