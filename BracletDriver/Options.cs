using OpenCvSharp;
using System.ComponentModel;
using System.Drawing;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BracletDriver;


public sealed class Options
{
    [Category("1. Камера"), DisplayName("Имя камеры")]
    public string CameraNameFilter { get; init; } = "Oracle Pro";

    [Category("1. Камера"), DisplayName("Ширина кадра")]
    public int CameraFrameWidth { get; init; } = 1920;

    [Category("1. Камера"), DisplayName("Высота кадра")]
    public int CameraFrameHeight { get; init; } = 1080;

    [Category("1. Камера"), DisplayName("Кадров в секунду")]
    public int CameraFps { get; init; } = 60;

    [Category("1. Камера"), DisplayName("Автофокус")]
    public bool CameraAutoFocus { get; init; } = false;

    [Category("1. Камера"), DisplayName("Фокус (ручной)")]
    public int CameraFocus { get; init; } = 20;

    [Category("1. Камера"), DisplayName("Пауза перед повтором, мс")]
    public int CameraRetryDelayMs { get; init; } = 200;

    [Category("1. Камера"), DisplayName("Ручной режим выдержки")]
    [Description("Что писать в AutoExposure, чтобы камера перестала подбирать выдержку сама. " +
             "Зависит от камеры: у одних 0, у других 0.25, у третьих 1.")]
    public double CameraManualExposureValue { get; set; } = 0.25;

    [Category("1. Камера"), DisplayName("Начальная выдержка")]
    [Description("У DSHOW это степень двойки: −6 ≈ 1/64 c, −7 ≈ 1/128 c. Шаг вдвое.")]
    public int CameraExposureStart { get; set; } = -6;

    [Category("1. Камера"), DisplayName("Минимальная выдержка")]
    public int CameraExposureMin { get; set; } = -13;

    [Category("1. Камера"), DisplayName("Кадров пропустить после смены выдержки")]
    public int CameraExposureSettleFrames { get; set; } = 3;

    [Category("2. Стержень"), DisplayName("Канал")]
    [Description("Порядок каналов BGR: 0 — синий, 1 — зелёный, 2 — красный. Берётся тот, " +
             "в котором свечение стержня ярче всего.")]
    public int RodChannel { get; set; } = 0;

    [Category("2. Стержень"), DisplayName("Порог яркости")]
    [Description("Пиксели ярче этого считаются стержнем. Работает в паре с выдержкой: она " +
                 "подбирается так, чтобы ярче порога не было ничего, кроме стержня.")]
    public int RodBrightThreshold { get; set; } = 128;

    [Category("2. Стержень"), DisplayName("Мин. площадь компонента")]
    [Description("Связные компоненты маски мельче этого (в пикселях) отбрасываются как блики и шум.")]
    public int RodMinComponentArea { get; set; } = 200;

    [Category("2. Стержень"), DisplayName("Предельная доля ярких пикселей")]
    [Description("Подбор выдержки заканчивается, когда ярче порога остаётся не больше этой доли " +
                 "кадра. Считать по максимуму нельзя: один битый пиксель увёл бы подбор в темноту.")]
    public double ExposureBrightShareLimit { get; set; } = 0.001;

    [Category("3. Разметка"), DisplayName("Ядро морфологических операций")]
    public int MorphKernelSize { get; init; } = 3;

    [Category("3. Разметка"), DisplayName("Мин. площадь контура")]
    public int MinContourArea { get; init; } = 500;

    [Category("3. Разметка"), DisplayName("Цвет рамки")]
    public Color BoxColor { get; init; } = Color.FromArgb(255, 0, 0);

    [Category("3. Разметка"), DisplayName("Толщина рамки")]
    public int BoxThickness { get; init; } = 2;

    [Category("3. Разметка"), DisplayName("Цвет нижней точки")]
    public Color TipColor { get; init; } = Color.FromArgb(0, 255, 0);

    [Category("3. Разметка"), DisplayName("Радиус нижней точки")]
    public int TipRadius { get; init; } = 5;


    [Browsable(false), JsonIgnore] public Scalar BoxScalar => ToScalar(BoxColor);
    [Browsable(false), JsonIgnore] public Scalar TipScalar => ToScalar(TipColor);

    private static Scalar ToScalar(Color c) => new(c.B, c.G, c.R);

    [Category("4. Вывод"), DisplayName("Папка вывода")]
    public string OutputDir { get; init; } = "C:\\Users\\annas\\source\\repos\\BracletDriver\\output\\";

    [Category("5. Привод"), DisplayName("COM-порт платы")]
    public string RodPortName { get; set; } = "COM5";

    [Category("5. Привод"), DisplayName("Скорость, бод")]
    public int RodBaudRate { get; set; } = 115200;

    [Category("5. Привод"), DisplayName("Скорость моторов")]
    public int RodSpeed { get; set; } = 2000;

    [Category("5. Привод"), DisplayName("Позиция: эталон")]
    public int[] RodBaselinePosition { get; set; } = [5080, 8280, 8280];

    [Category("5. Привод"), DisplayName("Позиция: промежуток")]
    public int[] RodMiddlePosition { get; set; } = [9889, 9859, 9859];

    [Category("5. Привод"), DisplayName("Позиция: рамка")]
    public int[] RodRoiPosition { get; set; } = [7887, 10245, 10245];

    [Category("5. Привод"), DisplayName("Разброс вокруг рамки, шагов")]
    public int RodRoiSpread { get; set; } = 300;

    [Category("5. Привод"), DisplayName("Позиция: дом")]
    public int[] RodHomePosition { get; set; } = [11844, 12872, 12872];

    [Category("5. Привод"), DisplayName("Исходное положение")]
    public int[] RodHome { get; set; } = [0, 450, -385];

    [Category("5. Привод"), DisplayName("Таймаут хода, мс")]
    public int RodMoveTimeoutMs { get; set; } = 3000;

    [Category("5. Привод"), DisplayName("Таймаут записи, мс")]
    public int RodWriteTimeoutMs { get; set; } = 1000;

    [Category("5. Привод"), DisplayName("Пауза между точками, мс")]
    public int RodDwellMs { get; set; } = 300;

    #region Поиск отверстий
    [Category("4. Поиск отверстий"), DisplayName("Размытие")]
    [Description("Размер ядра GaussianBlur перед заливкой. Меньше 3 — без размытия.")]
    public int Blur { get; set; } = 3;

    [Category("4. Поиск отверстий"), DisplayName("Порог темноты")]
    [Description("Яркость затравки: пиксели темнее этого значения считаются кандидатами.")]
    public int DarkMax { get; set; } = 100;

    [Category("4. Поиск отверстий"), DisplayName("Мин. площадь пятна")]
    public double MinArea { get; set; } = 10;

    [Category("4. Поиск отверстий"), DisplayName("Макс. площадь пятна")]
    public double MaxArea { get; set; } = 30;
    #endregion

    #region Поиск отверстий: адаптивный порог
    [Category("4. Поиск отверстий (адаптивный)"), DisplayName("Размер окна")]
    [Description("Сторона окна, по которому считается средняя яркость вокруг пикселя. Нечётное, " +
                 "примерно в 3 раза больше диаметра отверстия: окно, в котором отверстие занимает " +
                 "большую часть, само становится тёмным, и отверстие перестаёт отличаться от соседей. " +
                 "На пробных кадрах 31 подходило для отверстий 6–14 px, 61 — для 15–20 px.")]
    public int AdaptiveBlockSize { get; set; } = 31;

    [Category("4. Поиск отверстий (адаптивный)"), DisplayName("Контраст к соседям")]
    [Description("На сколько (0..255) пиксель должен быть темнее средней яркости окна, чтобы попасть " +
                 "в кандидаты. Меньше — в маску лезут текстура дерева и рифление браслетов, больше — " +
                 "пропадают бледные отверстия.")]
    public double AdaptiveC { get; set; } = 35;

    [Category("4. Поиск отверстий (адаптивный)"), DisplayName("Мин. площадь пятна")]
    public int AdaptiveMinArea { get; set; } = 10;

    [Category("4. Поиск отверстий (адаптивный)"), DisplayName("Макс. площадь пятна")]
    [Description("С запасом: из-за перспективы отверстия ближе к камере заметно крупнее дальних — " +
                 "на одном кадре площадь может отличаться в несколько раз.")]
    public int AdaptiveMaxArea { get; set; } = 300;

    [Category("4. Поиск отверстий (адаптивный)"), DisplayName("Макс. вытянутость")]
    [Description("Во сколько раз длинная сторона рамки пятна может превышать короткую. Отверстие почти " +
                 "круглое, прожилки дерева — вытянутые.")]
    public double AdaptiveMaxAspect { get; set; } = 1.6;

    [Category("4. Поиск отверстий (адаптивный)"), DisplayName("Мин. заполненность рамки")]
    [Description("Какую долю своей рамки должно занимать пятно. У круга это ~0.79, у рваных штрихов " +
                 "и уголков — заметно меньше.")]
    public double AdaptiveMinFill { get; set; } = 0.55;
    #endregion

    #region Сохранение
    public Options Clone() => (Options)MemberwiseClone();

    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "BracletDriver", "settings.json");

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
    [Category("3. Рабочая зона"), DisplayName("Левый нижний угол: X")]
    [Description("Одна рамка на всю обработку: в ней ищутся и отверстия с цепочками, и кончик " +
                 "стержня. Задаётся левым нижним углом, от которого вправо откладывается ширина, " +
                 "а вверх — высота. Нулевая ширина или высота — искать по всему кадру. Общая рамка " +
                 "намеренно: будь они разными, целевое отверстие могло бы оказаться вне зоны " +
                 "поиска стержня, и кончик стержня не совпал бы с ним никогда.")]
    public int RoiX { get; set; }

    [Category("3. Рабочая зона"), DisplayName("Левый нижний угол: Y")]
    [Description("В сырых пиксельных координатах, а не «визуально»: у кадра Y растёт вниз, поэтому " +
                 "нижний край — это большее значение Y, а верхний получается вычитанием высоты.")]
    public int RoiY { get; set; }

    [Category("3. Рабочая зона"), DisplayName("Ширина вправо от угла")]
    public int RoiWidth { get; set; }

    [Category("3. Рабочая зона"), DisplayName("Высота вверх от угла")]
    public int RoiHeight { get; set; }

    // OpenCV задаёт прямоугольник левым ВЕРХНИМ углом, поэтому здесь и только здесь высота
    // вычитается: наружу рамка описывается нижним углом, внутрь уходит уже в виде Rect.
    [Browsable(false), JsonIgnore]
    public Rect Roi => new(RoiX, RoiY - RoiHeight, RoiWidth, RoiHeight);
    #endregion

    #region Цепочки
    [Category("5. Цепочки"), DisplayName("Радиус поиска соседа")]
    public double ChainSearchRadius { get; set; } = 50;

    [Category("5. Цепочки"), DisplayName("Мин. точек в цепочке")]
    public int MinChainPoints { get; set; } = 4;

    [Category("5. Цепочки"), DisplayName("Мин. средний шаг")]
    public int MinChainAverageStep { get; set; } = 15;

    [Category("5. Цепочки"), DisplayName("Макс. средний шаг")]
    public int MaxChainAverageStep { get; set; } = 30;

    [Category("5. Цепочки"), DisplayName("Макс. поворот, град.")]
    [Description("На сколько градусов цепочка может отклониться на одном шаге — так она гнётся вместе с браслетом.")]
    public double MaxTurnAngleDeg { get; set; } = 5;

    [Category("5. Цепочки"), DisplayName("Макс. отношение площадей")]
    [Description("Насколько соседние отверстия могут отличаться по площади.")]
    public double MaxAreaRatio { get; set; } = 1.3;
    #endregion

    #region Наведение
    [Category("9. Наведение"), DisplayName("Пробный шаг калибровки")]
    [Description("На сколько крутить мотор при снятии таблицы. Должен быть крупным: сдвиг картинки " +
                 "обязан быть заметно больше шума детекции, иначе в таблицу запишется шум.")]
    public double GuidanceCalibrationStep { get; set; } = 200;

    [Category("9. Наведение"), DisplayName("Доля расчётного шага")]
    [Description("Какую часть от посчитанного по таблице шага делать. 1 — шаг целиком, без " +
                 "придерживания. Меньше единицы — страховка на случай, если таблица окажется " +
                 "неточной и полный шаг начнёт перелетать цель: до сходимости шагов больше, " +
                 "зато без раскачки около отверстия.")]
    public double GuidanceGain { get; set; } = 1.0;

    [Category("9. Наведение"), DisplayName("Максимальный шаг мотора, ед.")]
    [Description("Верхняя граница хода одного мотора за шаг коррекции. Шаг считается точным решением " +
                 "по таблице и при испортившейся таблице ничем не ограничен. Если предел превышен, " +
                 "весь шаг уменьшается в одно и то же число раз: направление сохраняется, просто " +
                 "проходится меньше. Это предохранитель, а не регулятор — значение должно быть заметно " +
                 "выше обычного рабочего хода, иначе шагов до цели станет больше без нужды. " +
                 "0 — без ограничения.")]
    public double GuidanceMaxStep { get; set; } = 1000;

    [Category("9. Наведение"), DisplayName("Радиус луча, px")]
    [Description("Насколько близко пиксель кончика стержня должен быть к пикселю отверстия, чтобы " +
                 "считать, что стержень на луче и можно опускать. Вышел за этот радиус — возвращаем обратно. " +
                 "Прямо задаёт итоговый промах: стержень может коснуться поверхности где угодно внутри " +
                 "этого радиуса, поэтому он должен быть не больше радиуса самого отверстия в пикселях. " +
                 "Мельче — точнее, но заметно больше шагов: на модели 8 px давали промах ~6 мм за 73 шага, " +
                 "4 px — ~1 мм за 80, а дальнейшее уменьшение только добавляло шаги, упираясь в шум детекции.")]
    public double GuidanceOnRayRadius { get; set; } = 4;

    [Category("9. Наведение"), DisplayName("Шаг спуска")]
    [Description("Насколько двигать стержень вдоль луча за один раз. Мельче — точнее момент касания, " +
                 "но больше шагов; крупнее — риск проскочить касание между двумя кадрами.")]
    public double GuidanceDescentStep { get; set; } = 100;

    [Category("9. Наведение"), DisplayName("Направление спуска")]
    [Description("С каким знаком брать направление вдоль луча камеры. Таблица задаёт его с точностью " +
                 "до знака — какая из двух сторон ведёт к столу, из неё не следует. Свойство стенда: " +
                 "подбирается один раз (пока камеру и моторы не переставляли, не меняется). " +
                 "Ошиблись знаком — стержень при «спуске» поедет вверх.")]
    public DescentSign GuidanceDescentSign { get; set; } = DescentSign.Plus;

    [Category("9. Наведение"), DisplayName("Мин. заметный сдвиг, px")]
    [Description("Если картинка сдвинулась меньше этого, таблица по такому шагу не правится: " +
                 "на таком масштабе движение неотличимо от шума детекции, и модель испортится.")]
    public double GuidanceMinObservedShift { get; set; } = 5;

    [Category("9. Наведение"), DisplayName("Макс. расхождение со шпаргалкой, px")]
    [Description("Если то, что предсказала таблица для сделанного хода, разошлось с тем, что " +
                 "вышло на самом деле, больше чем на это (в пикселях) — правка таблицы этим шагом " +
                 "пропускается: настолько большое расхождение за один шаг вероятнее означает сбой " +
                 "замера (кончик потерян, поймана не та точка, стержень ещё качается после резкого " +
                 "разворота хода), а не то, что таблица и правда настолько неточна. Без этого одна " +
                 "такая точка портит всю таблицу разом — смотреть за «ошибка N px» в логе шага, " +
                 "чтобы понять, какой порог нормален на конкретном стенде.")]
    public double GuidanceMaxResidual { get; set; } = 150;

    [Category("9. Наведение"), DisplayName("Пауза после шага, мс")]
    [Description("Стержень висит на тросах и после резкого хода мотора ещё качается — кадр, " +
                 "снятый сразу, поймал бы его в движении, а не в покое. Эта пауза идёт после каждого " +
                 "шага и перед замером, до того как кадр берётся для поиска кончика. 0 — без паузы.")]
    public int GuidanceSettleMs { get; set; } = 1000;

    [Category("9. Наведение"), DisplayName("Зона 1: сторона захода стержня")]
    [Description("С какой стороны от отверстия стержень заходит на цель. Свойство стенда — " +
                 "подбирается один раз (пока камеру и лампу не переставляли, не меняется).")]
    public Side RodApproachSide { get; set; } = Side.Up;

    [Category("9. Наведение"), DisplayName("Зона 1: сторона тени")]
    [Description("Куда от отверстия падает тень стержня. Свойство стенда — подбирается один раз " +
                 "(пока камеру и лампу не переставляли, не меняется). Должна лежать на другой оси, " +
                 "чем «Сторона захода стержня» (одна — вертикальная, другая — горизонтальная).")]
    public Side ShadowSide { get; set; } = Side.Left;

    [Category("9. Наведение"), DisplayName("MOG2: сторона кончика стержня")]
    [Description("В какую сторону кадра направлен сам стержень (в сырых пиксельных координатах) — " +
                 "по этой стороне MOG2Detector выбирает, какая крайняя точка скелета маски и есть " +
                 "кончик. Свойство стенда, подбирается один раз — не обязано совпадать с «Зона 1: " +
                 "сторона захода стержня» выше, это два разных механизма (яркостная проверка и MOG2).")]
    public Side RodTipSide { get; set; } = Side.Down;

    [Category("9. Наведение"), DisplayName("MOG2: сторона кончика обломка")]
    [Description("В какую сторону кадра направлен посторонний кусок рядом со стержнем (обычно — " +
                 "тень, ставшая у стола отдельным объектом на маске MOG2). Кончик ищется всегда у " +
                 "ближайшего к стержню контура, но сам по себе он ненадёжен (шум той же толщины) — " +
                 "доверять ли ему и как, решает вызывающий код (см. «Посадка» ниже).")]
    public Side ShadowTipSide { get; set; } = Side.Left;

    [Category("9. Наведение"), DisplayName("Зона 1: отступ от стержня, px")]
    [Description("Угловая проверка: от отверстия откладывается точка — против стороны захода " +
                 "стержня на это расстояние (чтобы отверстие оказалось между зоной и кончиком " +
                 "стержня), и против стороны тени на отдельное расстояние ниже. Одна из осей этой " +
                 "точки задаёт положение линии, другая — где линия заканчивается (у строки " +
                 "отверстия). Линия проверяется на изменение яркости относительно эталонного кадра. " +
                 "Как только сработала хоть раз, наведение переходит на более осторожные значения " +
                 "ниже — и уже не возвращается к обычным до конца прохода, даже если на следующем " +
                 "кадре зона больше не видна.")]
    public double GuidanceZone1RodOffset { get; set; } = 80;

    [Category("9. Наведение"), DisplayName("Зона 1: отступ от тени, px")]
    [Description("То же самое смещение, но против стороны тени, а не стержня.")]
    public double GuidanceZone1ShadowOffset { get; set; } = 80;

    [Category("9. Наведение"), DisplayName("Зона 1: толщина полосы, px")]
    [Description("Ширина полосы вдоль вертикальной линии, в которой считаются пиксели. Тоньше — " +
                 "точнее момент, толще — надёжнее против дрожания кадра и погрешности в один пиксель.")]
    public int GuidanceZone1LineWidth { get; set; } = 3;

    [Category("9. Наведение"), DisplayName("Зона 1: порог разницы яркости")]
    [Description("На сколько должна измениться яркость (0..255) относительно эталона, чтобы пиксель " +
                 "на линии считался изменившимся.")]
    public int GuidanceZone1DiffThreshold { get; set; } = 50;

    [Category("9. Наведение"), DisplayName("Зона 1: мин. пикселей")]
    [Description("Сколько пикселей на линии должны измениться ярче порога, чтобы засчитать это как " +
                 "срабатывание, а не как одиночный шум.")]
    public int GuidanceZone1MinPixels { get; set; } = 6;

    [Category("9. Наведение"), DisplayName("После зоны 1: радиус луча, px")]
    [Description("Замена «Радиуса луча» после первого срабатывания зоны 1 — тень уже рядом, и точнее " +
                 "держать стержень на луче важнее, чем идти быстро.")]
    public double GuidanceOnRayRadiusZone1 { get; set; } = 2;

    [Category("9. Наведение"), DisplayName("После зоны 1: шаг спуска")]
    [Description("Замена «Шага спуска» после первого срабатывания зоны 1 — мельче, чтобы точнее " +
                 "поймать момент касания, раз тень уже рядом с целью.")]
    public double GuidanceDescentStepZone1 { get; set; } = 25;

    [Category("9. Наведение"), DisplayName("После зоны 1: макс. шаг мотора, ед.")]
    [Description("Замена «Максимального шага мотора» после первого срабатывания зоны 1 — жёстче " +
                 "ограничивает шаг коррекции, чтобы не промахнуться мимо цели рывком, когда до неё " +
                 "уже недалеко.")]
    public double GuidanceMaxStepZone1 { get; set; } = 200;

    [Category("9. Наведение"), DisplayName("Посадка: расстояние до обломка, px")]
    [Description("Расстояние между кончиком стержня и ближайшим к нему посторонним куском на той же " +
                 "маске MOG2 (см. MOG2Detector.DetectRodTip) — пока стержень высоко, такого куска " +
                 "рядом нет вообще, тень мягкая и в маску не попадает; у стола тень становится " +
                 "резкой настолько, что MOG2 сам считает её объектом, и это расстояние падает по " +
                 "мере посадки. Как только оно не больше этого значения — финальный шаг и «успех», " +
                 "если при этом стержень на луче. Если нет — наведение возвращается в последнюю " +
                 "снятую позицию «на луче» и пробует снова. Проверяется только после того, как хоть " +
                 "раз сработала зона 1 — до этого рядом со стержнем может оказаться что угодно " +
                 "постороннее, а не именно тень у стола.")]
    public double GuidanceLandingGapPx { get; set; } = 15;

    [Category("9. Наведение"), DisplayName("После зоны 1: скорость моторов")]
    [Description("Замена «Скорости моторов» (категория «Привод») после первого срабатывания зоны 1 — " +
                 "медленнее, чтобы стержень меньше раскачивался на тросах рядом с целью и кадр после " +
                 "хода был чище.")]
    public int RodSpeedZone1 { get; set; } = 500;

    [Category("9. Наведение"), DisplayName("Максимум шагов")]
    [Description("Страховка от бесконечного кручения моторов.")]
    public int GuidanceMaxSteps { get; set; } = 300;

    [Category("9. Наведение"), DisplayName("Максимум кадров без стержня")]
    [Description("Сколько кадров подряд можно не видеть кончик стержня, прежде чем остановиться.")]
    public int GuidanceMaxLostFrames { get; set; } = 10;

    [Category("9. Наведение"), DisplayName("Таймаут свежего кадра, мс")]
    [Description("Камера читается фоновым потоком непрерывно; когда наведению нужен кадр, снятый " +
                 "заведомо после конкретного момента (например, после хода мотора), оно ждёт именно " +
                 "такой — не дольше этого времени. Если камера зависла, наведение узнает об этом " +
                 "через этот таймаут, а не будет ждать неопределённо долго.")]
    public int FreshFrameTimeoutMs { get; set; } = 2000;
    #endregion
}
