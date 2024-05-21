using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotWinForms.Models;

namespace TelegramBotWinForms
{
    public partial class Frm_General : Form
    {
        private static string botKey = "";
        private static TelegramBotClient botClient;


        public Frm_General()
        {
            InitializeComponent();
        }


        private void Frm_General_Load(object sender, EventArgs e)
        {
            //Проверяем наличие АПИ ключа
            if (string.IsNullOrEmpty(Properties.Settings.Default["BotApiKey"].ToString()))
                MessageBox.Show("Необходимо указать API Key bot");
            else
            {
                InitBot();
            }
        }


        /// <summary>
        /// Прослушиваем входящие сообщения
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private static bool isRunning = true;

        private async void BotClient_OnMessage(object sender, MessageEventArgs e)
        {
            string text = e.Message.Text;
            //Если текстовое сообщение
            if (e.Message.Type == Telegram.Bot.Types.Enums.MessageType.Text)
            {
                string outText;

                //Проверяем, является ли сообщение командой /stop
                if (text == "/stop")
                {
                    //Если да, то останавливаем бота
                    isRunning = false;
                    outText = "Бот остановлен";
                }
                else if (text == "/start")
                {
                    //Если да, то запускаем бота
                    isRunning = true;
                    
                    //Отправляем стикер
                    await botClient.SendStickerAsync(e.Message.Chat.Id, "CAACAgIAAxkBAVkBDGZMXyNIvYrufyX2KcqtIEfYs9aXAAKkFAACh_kxSPWBfmLe4SHzNQQ");

                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        new [] // первый ряд кнопок
                        {
                            InlineKeyboardButton.WithUrl("Микстейпы", "https://t.me/teleb0t1k_bot/mix_collection"),
                            InlineKeyboardButton.WithCallbackData("Подру-учный")
                        }
                    });
                    await botClient.SendTextMessageAsync(
                        chatId: e.Message.Chat.Id,
                        text: "Добро пожаловать! Давайте же начнём \nВыберите действие:",
                        replyMarkup: inlineKeyboard
                    );
                    outText = "Подручный нужен, если не охотно посмотреть на красивый сайт с кнопочками";
                }
                else if (text == "/help")
                {
                    outText = "Обращайтесь к видеоинструкии: <b>Фёдорова Алексея</b> \n https://www.youtube.com/playlist?list=PLBxD0UwW2SxmiogBXtVv5kItQXXQaPPKh";
                    await botClient.SendStickerAsync(e.Message.Chat.Id, "CAACAgIAAxkBAVkEEWZMY0rtbiBDF9S8jne7jr_2lYuWAALJFQACs4IwS-TSpNEFq-VhNQQ");
                }

                else if (text == "Микстейп 1")
                {
                    outText = "Ваша ссылка на Микстейп 1: https://t.me/teleb0t1k_bot/mix1";
                }
                else if (text == "Микстейп 2")
                {
                    outText = "Ваша ссылка на Микстейп 2: https://t.me/teleb0t1k_bot/mix2";
                }
                else if (text == "Микстейп 3")
                {
                    outText = "Ваша ссылка на Микстейп 3: https://t.me/teleb0t1k_bot/mix3";
                }
                else if (text == "Микстейп 4")
                {
                    outText = "Ваша ссылка на Микстейп 4: https://t.me/teleb0t1k_bot/mix4";
                }

                else
                {
                    //Если бот остановлен, то не обрабатываем другие сообщения
                    if (!isRunning)
                        return;

                    //Обрабатываем другие сообщения
                    outText = $"Имя: {e.Message.Chat.FirstName} \n <b>Сообщение:</b> {e.Message.Text}";
                    await botClient.SendStickerAsync(e.Message.Chat.Id, "CAACAgIAAxkBAVkEXmZMY_kIj8SD_1pzAdA_WhCKsFHFAAKGGgACSFExSxSikK5OEuI_NQQ");
                }

                //Отправляем ответ
                await botClient.SendTextMessageAsync(e.Message.Chat.Id, outText, parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
            }


        }




        /// <summary>
        /// Сохранение / изменение настроек
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_SaveSettings_Click(object sender, EventArgs e)
        {
            //Api Key
            if (!string.IsNullOrEmpty(tb_ApiKey.Text))
                Properties.Settings.Default["BotApiKey"] = tb_ApiKey.Text;

            //Сохранение настроек
            Properties.Settings.Default.Save();

            //Инициализация бота и программы с новыми настройками
            InitBot();
            MessageBox.Show("Настройки сохранены");
        }


        /// <summary>
        /// Завершение работы с ботом
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Frm_General_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("Закрыть программу?", "Подтверждение", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                botClient.StopReceiving();
            }
        }

        /// <summary>
        /// Инициализация бота
        /// </summary>
        private void InitBot()
        {

            if (botClient != null)
                botClient.StopReceiving();

            //Получаем Апи ключ клиента
            botKey = Properties.Settings.Default["BotApiKey"].ToString();
            tb_ApiKey.Text = botKey;

            //Инициализация бота
            try
            {
                botClient = new TelegramBotClient(botKey);
                botClient.StartReceiving();
                var me = botClient.GetMeAsync().GetAwaiter().GetResult();
                lbl_BotName.Text = me.FirstName;
                botClient.OnMessage += BotClient_OnMessage;

            }
            catch (Exception)
            {
                MessageBox.Show("Ошибка при инициализации бота. Необходимо проверить настройки или подключение к интернету");
            }
            botClient.OnCallbackQuery += BotOnCallbackQueryReceived;
        }

        private async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            var callbackQuery = callbackQueryEventArgs.CallbackQuery;

            if (callbackQuery.Data == "Подру-учный")
            {
                var keyboard = new ReplyKeyboardMarkup(new[]
                {
                    new[] // первый ряд кнопок
                    {
                        new KeyboardButton("Микстейп 1"),
                        new KeyboardButton("Микстейп 2")
                    },
                    new[] // второй ряд кнопок
                    {
                        new KeyboardButton("Микстейп 3"),
                        new KeyboardButton("Микстейп 4")
                    }
                })

                {
                    ResizeKeyboard = true // делаем клавиатуру адаптивной
                };

                await botClient.SendTextMessageAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    text: "Выберите микстейп",
                    replyMarkup: keyboard
                );
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }


    }
}
