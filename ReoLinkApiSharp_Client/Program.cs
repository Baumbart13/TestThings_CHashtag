// See https://aka.ms/new-console-template for more information

using System.Net;
using ReoLinkApiSharp;
using ReoLinkApiSharp.Handlers;

Console.WriteLine("Hello, World!");
var cameraUser = "admin";
var cameraUserPw = "Orangensaft";
var cameraIp = IPAddress.Parse("192.168.68.115");
Console.WriteLine($"Cam's IP is {cameraIp}");
var camera = new Camera(cameraIp, cameraUser, cameraUserPw);
