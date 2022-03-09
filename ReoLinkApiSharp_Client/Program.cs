// See https://aka.ms/new-console-template for more information

using System.Net;
using ReoLinkApiSharp;
using ReoLinkApiSharp.Handlers;
using SixLabors.ImageSharp;

Console.WriteLine("Hello, World!");
var cameraUser = "admin";
var cameraUserPw = "Orangensaft";
var cameraIp = IPAddress.Parse("192.168.68.100");
Console.WriteLine($"Cam's IP is {cameraIp}");
var camera = new Camera(cameraIp, cameraUser, cameraUserPw);
var img = camera.GetSnap();
img.SaveAsJpeg(@"C:\Users\Baumbart13\snap.jpg");