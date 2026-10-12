using OpenCvSharp;
using System.ComponentModel;
using System.Drawing;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BracletDriver;
internal sealed class Options
{
    [Category("01. Камера"), DisplayName("Имя камеры")]
    public string CameraNameFilter { get; set; } = "Oracle Pro";

    [Category("01. Камера"), DisplayName("Ширина кадра")]
    public int CameraFrameWidth { get; set; } = 1920;

    [Category("01. Камера"), DisplayName("Высота кадра")]
    public int CameraFrameHeight { get; set; } = 1080;

    [Category("01. Камера"), DisplayName("Кадров в секунду")]
    public int CameraFps { get; set; } = 60;

    [Category("01. Камера"), DisplayName("Автофокус")]
    public bool CameraAutoFocus { get; set; } = false;

    [Category("01. Камера"), DisplayName("Фокус (ручной)")]
    public int CameraFocus { get; set; } = 20;

    [Category("01. Камера"), DisplayName("Пауза перед повтором, мс")]
    public int CameraRetryDelayMs { get; set; } = 200;

    [Category("01. Камера"), DisplayName("Ручной режим выдержки")]
    [Description("Что писать в AutoExposure, чтобы камера перестала подбирать выдержку сама. " +
                 "Зависит от камеры: у одних 0, у других 0.25, у третьих 1.")]
    public double CameraManualExposureValue { get; set; } = 0.25;

    [Category("01. Камера"), DisplayName("Начальная выдержка")]
    [Description("Выдержка при открытии камеры, живого превью и сброса при выключении анализатора. " +
                 "У DSHOW это степень двойки: −6 ≈ 1/64 c, −7 ≈ 1/128 c. Шаг вдвое.")]
    public int CameraExposureStart { get; set; } = -6;

    [Category("01. Камера"), DisplayName("Выдержка для отверстий")]
    [Description("Выдержка, на которой ищется отверстие. Подбирается вручную кнопкой «Подобрать " +
                 "выдержку для отверстий» и запоминается в настройках.")]
    public int HoleExposure { get; set; } = -6;

    [Category("01. Камера"), DisplayName("Выдержка для стержня")]
    [Description("С неё начинается подбор перед выдвижением стержня: выдержка только понижается, " +
                 "пока самый яркий пиксель не станет ниже порога. Запоминается кнопкой «Подобрать " +
                 "выдержку для стержня».")]
    public int RodExposure { get; set; } = -6;

    [Category("01. Камера"), DisplayName("Минимальная выдержка")]
    public int CameraExposureMin { get; set; } = -13;

    [Category("01. Камера"), DisplayName("Кадров пропустить после смены выдержки")]
    public int CameraExposureSettleFrames { get; set; } = 3;

    [Category("02. Стержень"), DisplayName("Канал")]
    [Description("Порядок каналов BGR: 0 — синий, 1 — зелёный, 2 — красный. Берётся тот, " +
                 "в котором свечение стержня ярче всего.")]
    public int RodChannel { get; set; } = 0;

    [Category("02. Стержень"), DisplayName("Порог яркости")]
    [Description("Пиксели ярче этого считаются стержнем. Работает в паре с выдержкой: до " +
                 "выдвижения стержня она уменьшается, пока самый яркий пиксель кадра не станет ниже порога.")]
    public int RodBrightThreshold { get; set; } = 128;

    [Category("02. Стержень"), DisplayName("Ядро медианы при подборе выдержки")]
    [Description("Перед поиском самого яркого пикселя кадр сглаживается медианным фильтром, чтобы " +
                 "одиночные битые пиксели и искры не уводили выдержку в темноту. Нечётное, от 3.")]
    public int ExposureMedianKernelSize { get; set; } = 5;

    [Category("02. Стержень"), DisplayName("Мин. площадь компонента")]
    [Description("Связные компоненты маски мельче этого (в пикселях) отбрасываются как блики и шум.")]
    public int RodMinComponentArea { get; set; } = 200;

    [Category("04. Разметка"), DisplayName("Ядро морфологических операций")]
    public int MorphKernelSize { get; set; } = 3;

    [Category("04. Разметка"), DisplayName("Цвет рамки")]
    public Color BoxColor { get; set; } = Color.FromArgb(255, 0, 0);

    [Category("04. Разметка"), DisplayName("Толщина рамки")]
    public int BoxThickness { get; set; } = 2;

    [Category("04. Разметка"), DisplayName("Цвет нижней точки")]
    public Color TipColor { get; set; } = Color.FromArgb(0, 255, 0);

    [Category("04. Разметка"), DisplayName("Радиус нижней точки")]
    public int TipRadius { get; set; } = 5;

    [Category("04. Разметка"), DisplayName("Цвет цели")]
    public Color TargetColor { get; set; } = Color.FromArgb(255, 255, 0);

    [Category("04. Разметка"), DisplayName("Цвет окна яркости")]
    public Color ProbeColor { get; set; } = Color.FromArgb(255, 128, 0);

    [Category("04. Разметка"), DisplayName("Цвет найденного отверстия")]
    public Color HoleColor { get; set; } = Color.FromArgb(0, 200, 255);

    [Browsable(false), JsonIgnore] public Scalar BoxScalar => ToScalar(BoxColor);
    [Browsable(false), JsonIgnore] public Scalar TipScalar => ToScalar(TipColor);
    [Browsable(false), JsonIgnore] public Scalar TargetScalar => ToScalar(TargetColor);
    [Browsable(false), JsonIgnore] public Scalar ProbeScalar => ToScalar(ProbeColor);
    [Browsable(false), JsonIgnore] public Scalar HoleScalar => ToScalar(HoleColor);

    private static Scalar ToScalar(Color c) => new(c.B, c.G, c.R);

    [Category("06. Привод"), DisplayName("COM-порт платы")]
    public string RodPortName { get; set; } = "COM5";

    [Category("06. Привод"), DisplayName("Скорость, бод")]
    public int RodBaudRate { get; set; } = 115200;

    [Category("06. Привод"), DisplayName("Скорость моторов")]
    public int RodSpeed { get; set; } = 2000;

    [Category("06. Привод"), DisplayName("Позиция: эталон")]
    public int[] RodBaselinePosition { get; set; } = [5080, 8280, 8280];

    [Category("06. Привод"), DisplayName("Позиция: промежуток")]
    public int[] RodMiddlePosition { get; set; } = [9889, 9859, 9859];

    [Category("06. Привод"), DisplayName("Позиция: рамка")]
    public int[] RodRoiPosition { get; set; } = [7887, 10245, 10245];

    [Category("06. Привод"), DisplayName("Позиция: дом")]
    public int[] RodHomePosition { get; set; } = [11844, 12872, 12872];

    [Category("06. Привод"), DisplayName("Таймаут хода, мс")]
    public int RodMoveTimeoutMs { get; set; } = 3000;

    [Category("06. Привод"), DisplayName("Таймаут записи, мс")]
    public int RodWriteTimeoutMs { get; set; } = 1000;

    #region Поиск отверстий
    [Category("05. Поиск отверстий"), DisplayName("Размер окна")]
    [Description("Сторона окна, по которому считается средняя яркость вокруг пикселя. Нечётное, " +
                 "примерно в 3 раза больше диаметра отверстия: окно, в котором отверстие занимает " +
                 "большую часть, само становится тёмным, и отверстие перестаёт отличаться от соседей. " +
                 "На пробных кадрах 31 подходило для отверстий 6–14 px, 61 — для 15–20 px.")]
    public int AdaptiveBlockSize { get; set; } = 31;

    [Category("05. Поиск отверстий"), DisplayName("Контраст к соседям")]
    [Description("На сколько (0..255) пиксель должен быть темнее средней яркости окна, чтобы попасть " +
                 "в кандидаты. Меньше — в маску лезут текстура дерева и рифление браслетов, больше — " +
                 "пропадают бледные отверстия.")]
    public double AdaptiveC { get; set; } = 35;

    [Category("05. Поиск отверстий"), DisplayName("Мин. площадь пятна")]
    public int AdaptiveMinArea { get; set; } = 10;

    [Category("05. Поиск отверстий"), DisplayName("Макс. площадь пятна")]
    [Description("С запасом: из-за перспективы отверстия ближе к камере заметно крупнее дальних — " +
                 "на одном кадре площадь может отличаться в несколько раз.")]
    public int AdaptiveMaxArea { get; set; } = 300;

    [Category("05. Поиск отверстий"), DisplayName("Макс. вытянутость")]
    [Description("Во сколько раз длинная сторона рамки пятна может превышать короткую. Отверстие почти " +
                 "круглое, прожилки дерева — вытянутые.")]
    public double AdaptiveMaxAspect { get; set; } = 1.6;

    [Category("05. Поиск отверстий"), DisplayName("Мин. заполненность рамки")]
    [Description("Какую долю своей рамки должно занимать пятно. У круга это ~0.79, у рваных штрихов " +
                 "и уголков — заметно меньше.")]
    public double AdaptiveMinFill { get; set; } = 0.55;
    #endregion

    #region Сохранение
    // Массивы позиций копируются, иначе правка копии в окне настроек попала бы в оригинал
    public Options Clone()
    {
        var copy = (Options)MemberwiseClone();
        foreach (var property in typeof(Options).GetProperties())
            if (property.CanWrite && property.GetValue(this) is Array array)
                property.SetValue(copy, array.Clone());

        return copy;
    }

    // Переносит значения в этот же объект: на него ссылаются все, кто читает настройки
    public void CopyFrom(Options other)
    {
        foreach (var property in typeof(Options).GetProperties())
            if (property.CanRead && property.CanWrite)
                property.SetValue(this, property.GetValue(other));
    }

    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "BracletDriver", "settings.json");

    public static readonly string CalibrationPath = Path.Combine(
        Path.GetDirectoryName(SettingsPath)!, "calibration.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new ColorJsonConverter() },
    };

    // Загружает настройки, сохранённые прошлым запуском
    public static Options Load(out string? error)
    {
        error = null;
        if (!File.Exists(SettingsPath)) return new Options();

        try
        {
            string json = File.ReadAllText(SettingsPath);
            return JsonSerializer.Deserialize<Options>(json, JsonOptions) ?? new Options();
        }
        catch (Exception ex)
        {
            error = $"Не удалось загрузить настройки ({SettingsPath}): {ex.Message}. " +
                    "Использованы значения по умолчанию.";
            return new Options();
        }
    }

    public bool Save(out string? error)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
            File.WriteAllText(SettingsPath, JsonSerializer.Serialize(this, JsonOptions));
            error = null;
            return true;
        }
        catch (Exception ex)
        {
            error = $"Не удалось сохранить настройки: {ex.Message}";
            return false;
        }
    }

    private sealed class ColorJsonConverter : JsonConverter<Color>
    {
        public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            Color.FromArgb(reader.GetInt32());

        public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options) =>
            writer.WriteNumberValue(value.ToArgb());
    }
    #endregion

    #region Рабочая зона
    [Category("03. Рабочая зона"), DisplayName("Угол 1: X")]
    [Description("Одна рамка на всю обработку: в ней ищутся и отверстие, и кончик стержня. Задаётся " +
                 "двумя противоположными углами в пикселях полного кадра, в любом порядке. Если углы " +
                 "совпадают по X или по Y — рамки нет, поиск идёт по всему кадру.")]
    public int RoiX1 { get; set; }

    [Category("03. Рабочая зона"), DisplayName("Угол 1: Y")]
    [Description("Y растёт вниз, как у кадра.")]
    public int RoiY1 { get; set; }

    [Category("03. Рабочая зона"), DisplayName("Угол 2: X")]
    public int RoiX2 { get; set; }

    [Category("03. Рабочая зона"), DisplayName("Угол 2: Y")]
    public int RoiY2 { get; set; }

    [Browsable(false), JsonIgnore]
    public Rect Roi => Rect.FromLTRB(Math.Min(RoiX1, RoiX2), Math.Min(RoiY1, RoiY2),
                                     Math.Max(RoiX1, RoiX2), Math.Max(RoiY1, RoiY2));
    #endregion

    #region Цепочки
    [Category("07. Цепочки"), DisplayName("Радиус поиска соседа")]
    public double ChainSearchRadius { get; set; } = 50;

    [Category("07. Цепочки"), DisplayName("Мин. точек в цепочке")]
    public int MinChainPoints { get; set; } = 4;

    [Category("07. Цепочки"), DisplayName("Мин. средний шаг")]
    public int MinChainAverageStep { get; set; } = 15;

    [Category("07. Цепочки"), DisplayName("Макс. средний шаг")]
    public int MaxChainAverageStep { get; set; } = 30;

    [Category("07. Цепочки"), DisplayName("Макс. поворот, град.")]
    [Description("На сколько градусов цепочка может отклониться на одном шаге — так она гнётся вместе с браслетом.")]
    public double MaxTurnAngleDeg { get; set; } = 5;

    [Category("07. Цепочки"), DisplayName("Макс. отношение площадей")]
    [Description("Насколько соседние отверстия могут отличаться по площади.")]
    public double MaxAreaRatio { get; set; } = 1.3;
    #endregion

    #region Наведение
    [Category("08. Наведение"), DisplayName("Пробный шаг калибровки")]
    [Description("На сколько крутить мотор при снятии таблицы. Должен быть крупным: сдвиг картинки " +
                 "обязан быть заметно больше шума детекции, иначе в таблицу запишется шум.")]
    public double GuidanceCalibrationStep { get; set; } = 200;

    [Category("08. Наведение"), DisplayName("Доля расчётного шага")]
    [Description("Какую часть от посчитанного по таблице шага делать. 1 — шаг целиком, без " +
                 "придерживания. Меньше единицы — страховка на случай, если таблица окажется " +
                 "неточной и полный шаг начнёт перелетать цель: до сходимости шагов больше, " +
                 "зато без раскачки около отверстия.")]
    public double GuidanceGain { get; set; } = 1.0;

    [Category("08. Наведение"), DisplayName("Максимальный шаг мотора, ед.")]
    [Description("Верхняя граница хода одного мотора за шаг коррекции. Шаг считается точным решением " +
                 "по таблице и при испортившейся таблице ничем не ограничен. Если предел превышен, " +
                 "весь шаг уменьшается в одно и то же число раз: направление сохраняется, просто " +
                 "проходится меньше. Это предохранитель, а не регулятор — значение должно быть заметно " +
                 "выше обычного рабочего хода, иначе шагов до цели станет больше без нужды. " +
                 "0 — без ограничения.")]
    public double GuidanceMaxStep { get; set; } = 1000;

    [Category("08. Наведение"), DisplayName("Радиус луча, px")]
    [Description("Насколько близко пиксель кончика стержня должен быть к пикселю отверстия, чтобы " +
                 "считать, что стержень на луче и можно опускать. Вышел за этот радиус — возвращаем обратно. " +
                 "Прямо задаёт итоговый промах: стержень может коснуться поверхности где угодно внутри " +
                 "этого радиуса, поэтому он должен быть не больше радиуса самого отверстия в пикселях. " +
                 "Мельче — точнее, но заметно больше шагов: на модели 8 px давали промах ~6 мм за 73 шага, " +
                 "4 px — ~1 мм за 80, а дальнейшее уменьшение только добавляло шаги, упираясь в шум детекции.")]
    public double GuidanceOnRayRadius { get; set; } = 4;

    [Category("08. Наведение"), DisplayName("Шаг спуска")]
    [Description("Насколько двигать стержень вдоль луча за один раз. Мельче — точнее момент касания, " +
                 "но больше шагов; крупнее — риск проскочить касание между двумя кадрами.")]
    public double GuidanceDescentStep { get; set; } = 100;

    [Category("08. Наведение"), DisplayName("Направление спуска")]
    [Description("С каким знаком брать направление вдоль луча камеры. Таблица задаёт его с точностью " +
                 "до знака — какая из двух сторон ведёт к столу, из неё не следует. Свойство стенда: " +
                 "подбирается один раз (пока камеру и моторы не переставляли, не меняется). " +
                 "Ошиблись знаком — стержень при «спуске» поедет вверх.")]
    public DescentSign GuidanceDescentSign { get; set; } = DescentSign.Plus;

    [Category("08. Наведение"), DisplayName("Мин. заметный сдвиг, px")]
    [Description("Если картинка сдвинулась меньше этого, таблица по такому шагу не правится: " +
                 "на таком масштабе движение неотличимо от шума детекции, и модель испортится.")]
    public double GuidanceMinObservedShift { get; set; } = 5;

    [Category("08. Наведение"), DisplayName("Макс. расхождение со шпаргалкой, px")]
    [Description("Если то, что предсказала таблица для сделанного хода, разошлось с тем, что " +
                 "вышло на самом деле, больше чем на это (в пикселях) — правка таблицы этим шагом " +
                 "пропускается: настолько большое расхождение за один шаг вероятнее означает сбой " +
                 "замера (кончик потерян, поймана не та точка, стержень ещё качается после резкого " +
                 "разворота хода), а не то, что таблица и правда настолько неточна. Без этого одна " +
                 "такая точка портит всю таблицу разом — смотреть за «ошибка N px» в логе шага, " +
                 "чтобы понять, какой порог нормален на конкретном стенде.")]
    public double GuidanceMaxResidual { get; set; } = 150;

    [Category("08. Наведение"), DisplayName("Пауза после шага, мс")]
    [Description("Стержень висит на тросах и после резкого хода мотора ещё качается — кадр, " +
                 "снятый сразу, поймал бы его в движении, а не в покое. Эта пауза идёт после каждого " +
                 "шага и перед замером, до того как кадр берётся для поиска кончика. 0 — без паузы.")]
    public int GuidanceSettleMs { get; set; } = 1000;

    [Category("08. Наведение"), DisplayName("Сторона кончика стержня")]
    [Description("В какую сторону кадра направлен кончик стержня (в сырых пиксельных координатах). " +
                 "По ней детектор выбирает, какая крайняя точка скелета маски — кончик, а наведение " +
                 "ставит окно яркости: с этой стороны от отверстия тело стержня его не перекрывает.")]
    public Side RodTipSide { get; set; } = Side.Down;

    [Category("08. Наведение"), DisplayName("Окно яркости: отступ от отверстия, px")]
    [Description("Окно 3×3 ставится на это расстояние от центра отверстия в сторону кончика стержня. " +
                 "Должно быть заметно больше «Радиуса луча»: на спуске кончик стоит в пределах этого " +
                 "радиуса от центра и перекрывал бы близкое окно весь спуск.")]
    public int GuidanceProbeOffset { get; set; } = 12;

    [Category("08. Наведение"), DisplayName("Окно яркости: начало замедления")]
    [Description("На сколько (0..255) окно должно стать ярче, чем на кадре без стержня, чтобы шаг " +
                 "начал уменьшаться. Ниже — шум камеры, шаг полный.")]
    public double GuidanceSlowdownStart { get; set; } = 10;

    [Category("08. Наведение"), DisplayName("Окно яркости: полное замедление")]
    [Description("Прирост яркости окна, при котором шаг уменьшается до «Минимальной доли шага». " +
                 "Между началом и полным замедлением шаг уменьшается линейно.")]
    public double GuidanceSlowdownFull { get; set; } = 60;

    [Category("08. Наведение"), DisplayName("Минимальная доля шага")]
    [Description("До какой доли уменьшается шаг (и к лучу, и спуска), когда окно яркости засвечено полностью.")]
    public double GuidanceMinStepFraction { get; set; } = 0.2;

    [Category("08. Наведение"), DisplayName("Максимум шагов")]
    [Description("Страховка от бесконечного кручения моторов.")]
    public int GuidanceMaxSteps { get; set; } = 300;

    [Category("08. Наведение"), DisplayName("Максимум кадров без стержня")]
    [Description("Сколько кадров подряд можно не видеть кончик стержня, прежде чем остановиться.")]
    public int GuidanceMaxLostFrames { get; set; } = 10;
    #endregion
}
