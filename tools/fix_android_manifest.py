"""Repair duplicate provider authorities produced by the bundled Godot binary exporter.
Rebuild only the AXML string pool; preserve resource indices and all other chunks.
The resulting APK must be zipaligned and signed again (tools/build.sh does this).
"""
import pathlib
import struct
import sys
import zipfile


def fix_manifest(raw: bytes) -> bytes:
    data = bytearray(raw)
    pool_pos = 8
    kind, header_size, pool_size = struct.unpack_from('<HHI', data, pool_pos)
    assert kind == 1 and header_size == 28, 'Unsupported AXML string pool'
    count, styles, flags, start, style_start = struct.unpack_from('<5I', data, pool_pos+8)
    assert styles == 0 and flags & 256 == 0, 'Expected unstyled UTF-16 AXML'
    strings = []
    for i in range(count):
        offset = struct.unpack_from('<I', data, pool_pos+28+i*4)[0]
        at = pool_pos+start+offset
        length = struct.unpack_from('<H', data, at)[0]
        assert length < 0x8000, 'Unexpected long AXML string'
        strings.append(data[at+2:at+2+length*2].decode('utf-16-le'))
    providers = []
    offset = pool_pos+pool_size
    while offset < len(data):
        chunk_type, chunk_header, chunk_size = struct.unpack_from('<HHI', data, offset)
        assert chunk_size >= chunk_header >= 8
        if chunk_type == 0x102:
            element_id = struct.unpack_from('<I', data, offset+20)[0]
            if strings[element_id] == 'provider':
                attr_start, attr_size, attr_count = struct.unpack_from('<3H', data, offset+24)
                attrs = {}
                for i in range(attr_count):
                    at = offset+16+attr_start+i*attr_size
                    name_id, raw_id = struct.unpack_from('<II', data, at+4)
                    attrs[strings[name_id]] = (strings[raw_id] if raw_id != 0xffffffff else '',at)
                providers.append(attrs)
        offset += chunk_size
    startup = next(p for p in providers if p['name'][0] == 'androidx.startup.InitializationProvider')
    file_provider = next(p for p in providers if p['name'][0] == 'androidx.core.content.FileProvider')
    if startup['authorities'][0] != file_provider['authorities'][0]:
        print('Provider authorities already distinct; unchanged.')
        return raw
    value = file_provider['authorities'][0].removesuffix('.fileprovider')+'.androidx-startup'
    new_index = len(strings)
    strings.append(value)
    at = startup['authorities'][1]
    struct.pack_into('<I',data,at+8,new_index)
    assert data[at+15] == 3, 'Authority must be TYPE_STRING'
    struct.pack_into('<I',data,at+16,new_index)
    offsets, encoded = [], bytearray()
    for value in strings:
        offsets.append(len(encoded))
        payload = value.encode('utf-16-le')
        encoded += struct.pack('<H',len(payload)//2)+payload+b'\0\0'
    encoded += b'\0'*((-len(encoded))%4)
    new_start = 28+4*len(strings)
    new_size = new_start+len(encoded)
    # Appending a string means sorted flag cannot remain set.
    pool = struct.pack('<HHI5I',1,28,new_size,len(strings),0,flags & ~1,new_start,0)
    pool += struct.pack('<'+'I'*len(offsets),*offsets)+encoded
    result = bytearray(data[:pool_pos]+pool+data[pool_pos+pool_size:])
    struct.pack_into('<I',result,4,len(result))
    print('Repaired AndroidX startup provider authority.')
    return bytes(result)


def main() -> None:
    source, output = map(pathlib.Path,sys.argv[1:3])
    with zipfile.ZipFile(source) as src, zipfile.ZipFile(output,'w') as dst:
        for info in src.infolist():
            if info.filename.startswith('META-INF/') and info.filename.endswith(('.SF','.RSA','.DSA')):
                continue
            content = src.read(info)
            if info.filename == 'AndroidManifest.xml': content = fix_manifest(content)
            dst.writestr(info,content)

if __name__ == '__main__': main()
