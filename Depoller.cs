using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Stend_3
{
    public enum Tag
    {
        Reg0, 
        Reg1
    }

    /// <summary>
    /// Готов принимать сырые регистры по опросной модели и преобразовывать
    /// их в теги с уведомлением об изменениях по событийной модели
    /// </summary>
    /// <remarks>
    /// Для простоты привязки сделан динамическим объектом. Свойства = теги.
    /// </remarks>
    public class Depoller : DynamicObject, INotifyPropertyChanged
    {
        // Сюда новые и старые значения тегов
        IDictionary<Tag, Object> oldValues, newValues;

        // Сюда названия тегов, изменившихся на данном проходе
        List<Tag> dirtyTags = new List<Tag>();

        // Через это событие другие объекты узнают об изменениях
        public event PropertyChangedEventHandler PropertyChanged;

        Dispatcher dispatcher;

        public Depoller(Dispatcher d)
        {
            dispatcher = d;
            oldValues = new Dictionary<Tag, Object>();
            newValues = new Dictionary<Tag, Object>();
        }

        /// <summary>
        /// Разбирает Modbus-регистры на теги
        /// </summary>
        /// <param name="data">Modbus-регистры как есть</param>
        public void Input(ushort[] data)
        {
            // Парсим
            newValues[Tag.Reg0] = (double)data[0];
            newValues[Tag.Reg1] = (double)data[1];
            // Составляем список изменившихся
            lock (dirtyTags)
            {
                // Новый список
                dirtyTags.Clear();

                foreach (var pair in newValues)
                {
                    Object val = null;
                    // Если тег записывается впервые или если он изменился
                    if (!oldValues.TryGetValue(pair.Key, out val) || !pair.Value.Equals(val))
                    {
                        oldValues[pair.Key] = pair.Value; // Запоминаем на следующий раз
                        dirtyTags.Add(pair.Key); // Добавляем в список изменённый на этот раз
                    }
                }
            }
            if (PropertyChanged != null) // Если есть с кем поделиться этой радостной вестью
            {
                // В потоке пользовательского интерфейса
                dispatcher.BeginInvoke(new Action(delegate()
                {
                    lock (dirtyTags)
                    {
                        // Сообщить о каждом изменённом теге
                        foreach (var tag in dirtyTags)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs(tag.ToString()));
                        }
                    }
                }));
            }
        }

        /// <summary>
        /// Доступ к тегам как к свойствам объекта
        /// </summary>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return oldValues.TryGetValue((Tag)Enum.Parse(typeof(Tag), binder.Name), out result);
        }
    }
}
