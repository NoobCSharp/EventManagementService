namespace EventManagementService.Models
{
    public class Event
    {
        /// <summary>
        /// Идентификатор события.
        /// </summary>
        required public Guid EventId { get; set; }

        /// <summary>
        /// Название события.
        /// </summary>
        required public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Описание события.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Дата начала события.
        /// </summary>
        required public DateTime StartAt { get; set; }

        /// <summary>
        /// Дата окончания события.
        /// </summary>
        required public DateTime EndAt { get; set; }

        /// <summary>
        /// Общее количество мест на событие.
        /// </summary>
        required public int TotalSeats { get; set; }

        /// <summary>
        /// Текущее количество свободных мест.
        /// </summary>
        public int AvailableSeats { get; set; }

        /// <summary>
        /// Коллекция броней на событие
        /// </summary>
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();

        public Event()
        {
        }

        /// <summary>
        /// Пытается зарезервировать указанное количество мест для события.
        /// </summary>
        /// <param name="count">
        /// Количество мест для резервирования. По умолчанию 1.
        /// Ожидается положительное значение; при передаче 0 метод вернёт <c>true</c> и не изменит
        /// <see cref="AvailableSeats"/>, при передаче отрицательного значения <see cref="AvailableSeats"/>
        /// будет увеличено (вследствие вычитания отрицательного числа).
        /// </param>
        /// <returns>
        /// <c>true</c>, если доступных мест было достаточно и значение <see cref="AvailableSeats"/> уменьшено на <paramref name="count"/>;
        /// иначе <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Метод выполняет простую проверку наличия мест и не выполняет строгой валидации аргумента.
        /// Верхняя граница для значения <see cref="AvailableSeats"/> не контролируется в этом методе — при необходимости
        /// внешняя логика должна гарантировать корректность значений <see cref="TotalSeats"/> и <see cref="AvailableSeats"/>.
        /// Метод не является потокобезопасным: при конкурентных вызовах извне требуется обеспечить синхронизацию.
        /// </remarks>
        public bool TryReserveSeats(int count = 1)
        {
            if (count <= 0)
                throw new ArgumentException("Количество мест должно быть положительным!", nameof(count));

            if (AvailableSeats >= count)
            {
                AvailableSeats -= count;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Освобождает указанное количество мест для события, увеличивая значение <see cref="AvailableSeats"/>.
        /// </summary>
        /// <param name="count">
        /// Количество мест для освобождения. Должно быть положительным числом. По умолчанию 1.
        /// </param>
        /// <remarks>
        /// После увеличения значение <see cref="AvailableSeats"/> ограничивается сверху значением <see cref="TotalSeats"/>,
        /// чтобы не превысить общее количество мест. Метод не выполняет строгой проверки аргумента: при передаче
        /// отрицательного значения оно уменьшит <see cref="AvailableSeats"/>. Метод не является потокобезопасным;
        /// при одновременном доступе извне требуется обеспечить синхронизацию.
        /// </remarks>
        public void ReleaseSeats(int count = 1)
        {
            AvailableSeats += count;

            if (AvailableSeats > TotalSeats)
            {
                AvailableSeats = TotalSeats;
            }
        }
    }
}
