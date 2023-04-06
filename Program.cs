using TelitTfi;
using table.lib;

if (args.Length < 1 || args.Length > 2) {
    Console.WriteLine("Usage:");
    Console.WriteLine("\ttelit-tfi-extractor <tfi-file> [output-dir]");
    return;
}

string filename = args[0];
string outputDir = args.Length == 2 ? args[1] : "";

if (!File.Exists(filename)) {
    Console.WriteLine($"Error: Couldn't find file '{filename}'");
    return;
}

const int HeaderSize = 12;
const int ElementV1Size = 156;
//const int ElementV2Size = 1212;
const int FooterSize = 8;

using FileStream fs = File.OpenRead(filename);
BinaryReader br = new BinaryReader(fs);

if (fs.Length < HeaderSize + ElementV1Size + FooterSize) {
    Console.WriteLine($"Error: File is too small to be a TFI (needs to be at least {HeaderSize + ElementV1Size + FooterSize} bytes)");
    return;
}

fs.Seek(-FooterSize, SeekOrigin.End);

Footer footer = Footer.FromReader(br);

if (!footer.IsValid()) {
    Console.WriteLine("Error: Couldn't find TFI TOC footer");
    return;
}

fs.Seek(-(FooterSize + ElementV1Size), SeekOrigin.End);

Element lastElement = ElementV1.FromReader(br);

if (!lastElement.Group.Equals("TOC")) {
    Console.WriteLine($"Error: Last element doesn't have group 'TOC', found '{lastElement.Group}'");
    return;
}

if (!lastElement.Name.Equals("Last Element")) {
    Console.WriteLine($"Error: Last element doesn't have name 'Last Element', found '{lastElement.Name}'");
    return;
}

fs.Seek(lastElement.Offset, SeekOrigin.Begin);

Header header = Header.FromReader(br);

if (!header.IsValid()) {
    Console.WriteLine("Error: Couldn't find TFI TOC header");
    return;
}

uint checksum = br
    .ReadBytes((int)lastElement.Length - (HeaderSize + FooterSize))
    .Aggregate((uint)0, (checksum, b) => checksum + b);

if (checksum != footer.Checksum) {
    Console.WriteLine($"Error: Checksum mismatch, calculated: 0x{checksum:X08}, read: {footer.Checksum:X08}");
    return;
}

List<Element> elements = new List<Element>();

fs.Seek(lastElement.Offset + HeaderSize, SeekOrigin.Begin);

for (int i = 0; i < header.NumElements; i++) {
    if (header.Version == 1 || i == header.NumElements - 1) {
        elements.Add(ElementV1.FromReader(br));
    } else {
        elements.Add(ElementV2.FromReader(br));
    }
}

Table<Element>
    .Add(elements)
    .ToConsole();

foreach (Element element in elements) {
    if (element.Length == 0xFFFFFFFF) continue;

    string path = Path.GetFullPath(Path.Combine(outputDir, element.Name.TrimStart('/')));
    string? dir = Path.GetDirectoryName(path);
    if (dir == null) continue;
    
    Directory.CreateDirectory(dir);
    using FileStream outstream = File.Create(path);

    fs.Seek(element.Offset, SeekOrigin.Begin);
    byte[] data = br.ReadBytes((int)element.Length);
    outstream.Write(data, 0, data.Length);
}