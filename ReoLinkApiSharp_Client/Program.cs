// See https://aka.ms/new-console-template for more information

using System.Net;
using ReoLinkApiSharp;

Console.WriteLine("Hello, World!");
var cameraIp = IPAddress.Parse("192.168.68.115");
Console.WriteLine($"Cam's IP is {cameraIp}");
var camera = new Camera(cameraIp, "admin", "Orangensaft");
