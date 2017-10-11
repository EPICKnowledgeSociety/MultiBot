﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using WindowsInput;
using WindowsInput.Native;
using EnvControllers;
using SimpleTCP;
using System.IO;
using Enigma.D3.ApplicationModel;
using Enigma.D3.Assets;
using Enigma.D3.MemoryModel;
using Enigma.D3.MemoryModel.Core;
using Enigma.D3;
using Enigma.D3.MemoryModel.Controls;
using static Enigma.D3.MemoryModel.Core.UXHelper;
using SlimDX.DirectInput;
using System.Runtime.InteropServices;

namespace TestLogClassl
{
    class Program
    {
       

        static void Main(string[] args)
        {
            string pathToFile = @"C:\Users\GuilhermeMarques\Documents\RoS-BoT\Logs\logs.txt";
            RosController rosCon = new RosController(pathToFile);

            ServerController server = new ServerController();
            server.port = 8910;
            server.pathToLogFile = @"C:\Users\GuilhermeMarques\Documents\RoS-BoT\Logs\logs.txt";
            server.Start();
            server.StartModules();

            //ClientController client = new ClientController();
            //client.serverip = "127.0.0.1";
            //client.serverport = 8910;
            //client.Start();
            //client.sendMessage("StartModules");
            while (true) {
                server.gameState.UpdateGameState();
                var newLogLines = server.rosController.rosLog.NewLines;
                
                if (LogFile.LookForString(newLogLines, "Vendor Loop Done") & !server.rosController.vendorLoopDone) {
                    //pause after vendor loop done
                    server.rosController.vendorLoopDone = true;
                    server.rosController.enteredRift = false;
                    server.sendMessage("Vendor Loop Done");
                    if (server.rosController.otherVendorLoopDone == false)
                    {
                        server.rosController.Pause();
                    }
                    Thread.Sleep(100);
                }

                if (LogFile.LookForString(newLogLines, "Next rift in different") & !server.gameState.inMenu)
                {   
                    //failure detected
                    server.sendMessage("Go to menu");
                    server.GoToMenu();
                    Thread.Sleep(500);
                }

                if (server.gameState.acceptgrUiVisible & server.rosController.vendorLoopDone) {
                    // click accept
                    server.rosController.enteredRift = false;
                    var xCoord = server.gameState.acceptgrUiControl.uirect.TranslateToClientRect(server.gameState.clientWidth, server.gameState.clientHeight).Left +
                        (server.gameState.acceptgrUiControl.uirect.TranslateToClientRect(server.gameState.clientWidth, server.gameState.clientHeight).Width / 2);
                    var yCoord = server.gameState.acceptgrUiControl.uirect.TranslateToClientRect(server.gameState.clientWidth, server.gameState.clientHeight).Top +
                        (server.gameState.acceptgrUiControl.uirect.TranslateToClientRect(server.gameState.clientWidth, server.gameState.clientHeight).Height / 2);
                    RosController.SetCursorPos((int)xCoord, (int)yCoord);
                    server.rosController.inputSimulator.Mouse.LeftButtonClick();                    
                    Thread.Sleep(1500);                   
                }

                if (server.gameState.cancelgriftUiVisible)
                {
                    //click cancel ok
                    server.rosController.Pause();
                    var xCoord = server.gameState.confirmationUiControl.uirect.TranslateToClientRect(server.gameState.clientWidth, server.gameState.clientHeight).Left +
                        (server.gameState.confirmationUiControl.uirect.TranslateToClientRect(server.gameState.clientWidth, server.gameState.clientHeight).Width / 2);
                    var yCoord = server.gameState.confirmationUiControl.uirect.TranslateToClientRect(server.gameState.clientWidth, server.gameState.clientHeight).Top +
                        (server.gameState.confirmationUiControl.uirect.TranslateToClientRect(server.gameState.clientWidth, server.gameState.clientHeight).Height / 2);
                    RosController.SetCursorPos((int)xCoord, (int)yCoord);
                    server.rosController.inputSimulator.Mouse.LeftButtonClick();
                    server.sendMessage("Start");
                    Thread.Sleep(1500);
                }

                if (server.gameState.firstlevelRift & !server.rosController.enteredRift)
                {
                    //unpause after entering rift
                    Thread.Sleep(1500);
                    server.rosController.enteredRift = true;
                    server.rosController.Unpause();
                    server.rosController.InitVariables();
                    Thread.Sleep(500);
                }

                if (server.gameState.haveUrshiActor)
                {   
                    //set Urshi state
                    server.rosController.didUrshi = true;
                }
                else
                {
                    server.rosController.didUrshi = false;
                }
            }
        }

    }
}
