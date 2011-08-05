using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace phystest
{
    public class Console
    {
        static Console consoleInstance;
        Dictionary<String, bool> booleanValues;
        Dictionary<String, float> floatValues;
        Dictionary<String, int> integerValues;
        ContentManager Content;        
        Texture2D borderImage, tlcornerImage, brcornerImage, trcornerImage, blcornerImage, bgImage;
        SpriteFont consoleFont;
        ConsoleHistory commandHistory;
        ConsoleHistory messageHistory;
        String currentCommand;
        List<String> lines;
        KeyboardState lastState;

        const int charWidth = 8;
        const int charHeight = 14;

        private Console()
        {
            booleanValues = new Dictionary<string, bool>();
            floatValues = new Dictionary<string, float>();
            integerValues = new Dictionary<string, int>();
            integerValues.Add("consoleHeight", 512);
            integerValues.Add("consoleWidth", 768);
            integerValues.Add("consolePositionX", 64);
            integerValues.Add("consolePositionY", 64);
            integerValues.Add("consoleBorderWidth", 16);
            integerValues.Add("consoleAlpha", 220);
            booleanValues.Add("consoleActive", false);
            commandHistory = new ConsoleHistory(120);
            messageHistory = new ConsoleHistory(120);
            lines = new List<String>();
            currentCommand = "";
            lastState = new KeyboardState();
            booleanValues.Add("texInfo", false);
            booleanValues.Add("meshCountInfo", false);
            booleanValues.Add("showPosition", false);
            booleanValues.Add("occluderInfo", false);
            booleanValues.Add("drawshadowtex", false);
            booleanValues.Add("glow", true);
            floatValues.Add("hdrpower", 4.0f);
        }
        static Console()
        {
            if (consoleInstance == null)
                consoleInstance = new Console();            
        }
        void InstanceHandleInput(KeyboardState keyboardState)
        {
            if (keyboardState.IsKeyDown(Keys.Up) && !lastState.IsKeyDown(Keys.Up))
            {

            }
            if (keyboardState.IsKeyDown(Keys.Down) && !lastState.IsKeyDown(Keys.Down))
            {

            }
            if (keyboardState.IsKeyDown(Keys.Tab) && !lastState.IsKeyDown(Keys.Tab))
            {

            }

            foreach (Char ch in Game1.kbBuffer.GetText())
            {
                if (ch == '\b')
                {
                    if (currentCommand.Length > 0)
                        currentCommand = currentCommand.Substring(0, currentCommand.Length - 1);
                }
                else if (ch == '\n')
                {
                    Console.Parse(currentCommand);
                    currentCommand = String.Empty;
                }
                else if (ch == '`')
                {
                    Console.Close();
                }
                else
                {
                    currentCommand += ch;
                }
            }
            lastState = keyboardState;
        }
        public static void HandleInput(KeyboardState keyboardState)
        {
            consoleInstance.InstanceHandleInput(keyboardState);
        }

        #region Open/Close

        void InstanceClose()
        {
            Game1.kbBuffer.Enabled = false;
            Game1.kbBuffer.TranslateMessage = false;
            SetBool("consoleActive", false);
        }

        public static void Close()
        {
            consoleInstance.InstanceClose();
        }

        void InstanceOpen()
        {
            Game1.kbBuffer.Enabled = true;
            Game1.kbBuffer.TranslateMessage = true;
            SetBool("consoleActive", true);
        }

        public static void Open()
        {
            consoleInstance.InstanceOpen();
        }

        #endregion

        #region LoadContent
        void InstanceLoadContent(ContentManager contentManager)
        {
            Content = contentManager;
            borderImage = Content.Load<Texture2D>("textures/Console/border");
            tlcornerImage = Content.Load<Texture2D>("textures/Console/cornertl");
            trcornerImage = Content.Load<Texture2D>("textures/Console/cornertr");
            blcornerImage = Content.Load<Texture2D>("textures/Console/cornerbl");
            brcornerImage = Content.Load<Texture2D>("textures/Console/cornerbr");
            bgImage = Content.Load<Texture2D>("textures/Console/bgcolor");
            consoleFont = Content.Load<SpriteFont>("Lucida");
        }
        public static void LoadContent(ContentManager contentManager)
        {
            consoleInstance.InstanceLoadContent(contentManager);
        }
        #endregion

        #region Draw
        void InstanceDraw(SpriteBatch spriteBatch)
        {            
            int height = GetInt("consoleHeight");
            int width = GetInt("consoleWidth");
            height = height < 128 ? 128 : height;
            width = width < 128 ? 128 : width;
            int posX = GetInt("consolePositionX");
            int posY = GetInt("consolePositionY");
            int borderWidth = GetInt("consoleBorderWidth");

            int numlines = (height - (borderWidth * 2)) / charHeight;
            int numchars = (width - (borderWidth * 2)) / charWidth;

            Rectangle tlcorner = new Rectangle(posX, posY, borderWidth, borderWidth);
            Rectangle trcorner = new Rectangle(width - borderWidth + posX, posY, borderWidth, borderWidth);
            Rectangle blcorner = new Rectangle(posX, height - borderWidth + posY, borderWidth, borderWidth);
            Rectangle brcorner = new Rectangle(width - borderWidth + posX, height - borderWidth + posY, borderWidth, borderWidth);
            Rectangle lborder = new Rectangle(posX, posY + borderWidth, borderWidth, height - (borderWidth * 2));
            Rectangle rborder = new Rectangle(width - borderWidth + posX, posY + borderWidth, borderWidth, height - (borderWidth * 2));
            Rectangle tborder = new Rectangle(width + posX - borderWidth, posY, borderWidth, width - (borderWidth * 2));
            Rectangle bborder = new Rectangle(width + posX - borderWidth, height - borderWidth + posY, borderWidth, width - (borderWidth * 2));
            Rectangle bgimg = new Rectangle(borderWidth + posX, borderWidth + posY, width - (borderWidth * 2), height - (borderWidth * 2));

            spriteBatch.Draw(tlcornerImage, tlcorner, Color.White);
            spriteBatch.Draw(brcornerImage, brcorner, Color.White);
            spriteBatch.Draw(trcornerImage, trcorner, Color.White);
            spriteBatch.Draw(blcornerImage, blcorner, Color.White);
           
            spriteBatch.Draw(borderImage, lborder, Color.White);
            spriteBatch.Draw(borderImage, rborder, Color.White);
            spriteBatch.Draw(borderImage, bborder, null, Color.White, MathHelper.PiOver2, Vector2.Zero, SpriteEffects.None, 0.0f);
            spriteBatch.Draw(borderImage, tborder, null, Color.White, MathHelper.PiOver2, Vector2.Zero, SpriteEffects.None, 0.0f);
            spriteBatch.Draw(bgImage, bgimg, new Color(255, 255, 255, Console.GetInt("consoleAlpha")));

            spriteBatch.DrawString(consoleFont, currentCommand, new Vector2(posX + borderWidth, posY + height - charHeight - borderWidth), Color.White);

        }
        public static void Draw(SpriteBatch spriteBatch)
        {
            if (Console.GetBool("consoleActive"))
                consoleInstance.InstanceDraw(spriteBatch);
        }
        #endregion 

        #region console variables

        public static void AddBool(String varName, bool value)
        {
            consoleInstance.booleanValues.Add(varName, value);
        }
        public static void AddFloat(String varName, float value)
        {
            consoleInstance.floatValues.Add(varName, value);
        }
        public static void AddInt(String varName, int value)
        {
            consoleInstance.integerValues.Add(varName, value);
        }
        public static bool GetBool(String varName)
        {            
            return consoleInstance.booleanValues[varName];            
        }
        public static float GetFloat(String varName)
        {
            return consoleInstance.floatValues[varName];
        }
        public static int GetInt(String varName)
        {
            return consoleInstance.integerValues[varName];
        }
        public static void SetBool(String varName, bool value)
        {
            consoleInstance.booleanValues[varName] = value;
        }
        public static void SetFloat(String varName, float value)
        {
            consoleInstance.floatValues[varName] = value;
        }
        public static void SetInt(String varName, int value)
        {
            consoleInstance.integerValues[varName] = value;
        }
        public static void Toggle(String varName)
        {            
            consoleInstance.booleanValues[varName] = !consoleInstance.booleanValues[varName];
            if (varName == "consoleActive")
            {
                if (consoleInstance.booleanValues[varName])
                    Console.Open();
                else
                    Console.Close();
            }
        }
        public static bool Parse(String toBeParsed)
        {
            String[] words = toBeParsed.Split(' ');
            if (words.Length > 1)
            {
                if (consoleInstance.booleanValues.ContainsKey(words[0]))
                {
                    bool result;
                    if (Boolean.TryParse(words[1], out result))
                    {
                        consoleInstance.booleanValues[words[0]] = result;
                        return true;
                    }
                    else if (words[1] == "1")
                    {
                        consoleInstance.booleanValues[words[0]] = true;
                        return true;
                    }
                    else if (words[1] == "0")
                    {
                        consoleInstance.booleanValues[words[0]] = false;
                        return true;
                    }
                }
                if (consoleInstance.floatValues.ContainsKey(words[0]))
                {
                    float result;
                    Single.TryParse(words[1], out result);
                    consoleInstance.floatValues[words[0]] = result;
                    return true;
                }
                if (consoleInstance.integerValues.ContainsKey(words[0]))
                {
                    int result;
                    Int32.TryParse(words[1], out result);
                    consoleInstance.integerValues[words[0]] = result;
                    return true;
                }

                bool bresult;
                if (Boolean.TryParse(words[1], out bresult))
                {
                    Console.AddBool(words[0], bresult);
                    return true;
                }
                else if (words[1] == "1")
                {
                    Console.AddBool(words[0], true);
                    return true;
                }
                else if (words[1] == "0")
                {
                    Console.AddBool(words[0], false);
                    return true;
                }

                float fresult;
                if (Single.TryParse(words[1], out fresult))
                {
                    Console.AddFloat(words[0], fresult);
                    return true;
                }
            }
            return false;
        }
        #endregion
    }
        
}
