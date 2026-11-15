"""Configure model catalogs and extend the existing Hair UI row format."""
import re
import uuid
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1] / 'DEMO RING'
ASSETS = ROOT / 'Assets'
SCRIPTS = ASSETS / 'Scripcts/Game Saving/Appearance'
DATA = ASSETS / 'Data/Appearance'


def write(path, value):
    path.write_text(value, encoding='utf-8', newline='\n')


def meta(path, script=False):
    p = Path(str(path) + '.meta')
    if p.exists():
        return re.search(r'^guid: (\w+)', p.read_text(), re.M)[1]
    guid = uuid.uuid5(uuid.NAMESPACE_URL, 'demo-ring/' + path.relative_to(ASSETS).as_posix()).hex
    importer = ('MonoImporter:\n  externalObjects: {}\n  serializedVersion: 2\n'
                '  defaultReferences: []\n  executionOrder: 0\n  icon: {instanceID: 0}\n') if script else (
                'NativeFormatImporter:\n  externalObjects: {}\n  mainObjectFileID: 11400000\n')
    write(p, f'fileFormatVersion: 2\nguid: {guid}\n{importer}  userData: \n  assetBundleName: \n  assetBundleVariant: \n')
    return guid


def blocks(text):
    return {m[2]: (m[1], m[3]) for m in re.finditer(
        r'^--- !u!(\d+) &(-?\d+)\n(.*?)(?=^--- !u!|\Z)', text, re.M | re.S)}


def replace_block(text, key, body):
    pattern = r'(^--- !u!\d+ &' + key + r'\n).*?(?=^--- !u!|\Z)'
    result, n = re.subn(pattern, lambda m: m[1] + body, text, flags=re.M | re.S)
    assert n == 1, key
    return result


def save_catalog(name, script_name, list_name, options):
    guid = meta(SCRIPTS / (script_name + '.cs'), True)
    path = DATA / (name + '.asset')
    text = ('%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n--- !u!114 &11400000\n'
            'MonoBehaviour:\n  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n'
            '  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n'
            '  m_GameObject: {fileID: 0}\n  m_Enabled: 1\n  m_EditorHideFlags: 0\n'
            f'  m_Script: {{fileID: 11500000, guid: {guid}, type: 3}}\n'
            f'  m_Name: {name}\n  m_EditorClassIdentifier: \n  {list_name}:\n')
    for option in options:
        for i, (key, value) in enumerate(option.items()):
            text += ('  - ' if i == 0 else '    ') + f'{key}: {value}\n'
    write(path, text)
    return meta(path)


for script in SCRIPTS.glob('*.cs'):
    meta(script, True)

player_path = ASSETS / 'Data/Prefab/Player/Player.prefab'
player_text = player_path.read_text(encoding='utf-8')
pb = blocks(player_text)


def child_names(parent_go):
    tr = re.search(r'component: \{fileID: (\d+)\}', pb[parent_go][1])[1]
    section = pb[tr][1].split('m_Children:')[1].split('m_Father:')[0]
    names = []
    for child_tr in re.findall(r'fileID: (\d+)', section):
        go = re.search(r'm_GameObject: \{fileID: (\d+)\}', pb[child_tr][1])[1]
        names.append(re.search(r'^  m_Name: (.*)', pb[go][1], re.M)[1])
    return sorted(names)


hair_names = child_names('740765748149725341')
head_names = child_names('8265714524795381650')
female_heads = child_names('3155342132077827442')
male_brows = child_names('8949711754262185676')
female_brows = child_names('3832053291758408534')
beards = child_names('7930679284894636559')
hair_guid = save_catalog('HairStyleCatalog', 'HairStyleCatalog', 'hairStyleOptions',
    [dict(hairID='none', hairName='No_Hair', hairModelName='')] + [
    dict(hairID='hair'+n[-2:], hairName='Hair_'+n[-2:], hairModelName=n) for n in hair_names])
assert [n.replace('_Male_', '_Female_') for n in head_names] == female_heads
head_guid = save_catalog('HeadStyleCatalog', 'HeadStyleCatalog', 'headStyleOptions', [
    dict(headID='head'+n[-2:], headName='Face_'+n[-2:], maleHeadModelName=n,
         femaleHeadModelName=n.replace('_Male_', '_Female_')) for n in head_names])
brow_guids = []
for sex, names in [('Male', male_brows), ('Female', female_brows)]:
    brow_guids.append(save_catalog(sex+'EyebrowStyleCatalog', 'EyebrowStyleCatalog', 'eyebrowStyleOptions',
        [dict(eyebrowID='none', eyebrowName='No_Eyebrows', eyebrowModelName='')] + [
        dict(eyebrowID='eyebrow'+n[-2:], eyebrowName='Eyebrows_'+n[-2:], eyebrowModelName=n) for n in names]))
beard_guid = save_catalog('FacialHairStyleCatalog', 'FacialHairStyleCatalog', 'facialHairStyleOptions',
    [dict(facialHairID='none', facialHairName='No_Beard', facialHairModelName='')] + [
    dict(facialHairID='beard'+n[-2:], facialHairName='Beard_'+n[-2:], facialHairModelName=n) for n in beards])

body = pb['1903931729856076015'][1]
# Head references now point to the variant containers, matching the existing Hair reference.
body = re.sub(r'^  maleHead: .*$', '  maleHead: {fileID: 8265714524795381650}', body, flags=re.M)
body = re.sub(r'^  femaleHead: .*$', '  femaleHead: {fileID: 3155342132077827442}', body, flags=re.M)
for field, guid in [('headStyleCatalog', head_guid), ('maleEyebrowStyleCatalog', brow_guids[0]),
                    ('femaleEyebrowStyleCatalog', brow_guids[1]), ('facialHairStyleCatalog', beard_guid)]:
    assert not re.search(r'^  '+field+':', body, re.M), 'Already configured: '+field
    body += f'  {field}: {{fileID: 11400000, guid: {guid}, type: 2}}\n'
body += ('  currentHairID: none\n  currentHeadID: head00\n  currentMaleEyebrowID: eyebrow01\n'
         '  currentFemaleEyebrowID: eyebrow01\n  currentFacialHairID: none\n')
player_text = replace_block(player_text, '1903931729856076015', body)
# Seed the default variants for both sexes without changing container visibility.
for go in ['2447244985000916034', '5711834954198312511', '1575824336998719091', '8230758361737351405']:
    player_text = replace_block(player_text, go, pb[go][1].replace('  m_IsActive: 0', '  m_IsActive: 1'))
write(player_path, player_text)

ui_path = ASSETS / 'Data/Prefab/UI/Title Screen Canvas.prefab'
ui = ui_path.read_text(encoding='utf-8')
ub = blocks(ui)
source_ids = ['8412264050774023017', '2514999396228224437', '3518986989724620453',
              '5271934433521923074', '7697728718566474877', '6523012470636548779',
              '3689816065741891369', '8732283707183048496', '708161490383647075', '2175113789463330981']
new_rows = []
for index, (name, method, label) in enumerate([
        ('Face', 'SelectHeadStyle', 'FACE < Face_00 >'),
        ('Eyebrows', 'SelectEyebrowStyle', 'EYEBROWS < Eyebrows_01 >'),
        ('Beard', 'SelectFacialHairStyle', 'BEARD < No_Beard >')], 1):
    mapping = {old: str(7800000000000000000 + index * 100 + j) for j, old in enumerate(source_ids)}
    for old in source_ids:
        typ, content = ub[old]
        # SelectedOnEnable stays on Hair only, so clones cannot steal the initial focus.
        content = content.replace('  - component: {fileID: 5564234601226403651}\n', '')
        content = re.sub(r'fileID: (\d+)', lambda m: 'fileID: '+mapping.get(m[1], m[1]), content)
        if old == source_ids[0]:
            content = content.replace('  m_Name: Hair\n', f'  m_Name: {name}\n')
        if old == '6523012470636548779':
            content = content.replace('m_MethodName: SelectHairStyle', 'm_MethodName: '+method)
        if old == '2175113789463330981':
            content = re.sub(r'^  m_text: .*$', '  m_text: '+label, content, flags=re.M)
            content = content.replace('  m_enableAutoSizing: 0', '  m_enableAutoSizing: 1')
            content = content.replace('  m_fontSizeMax: 72', '  m_fontSizeMax: 24')
        ui += f'--- !u!{typ} &{mapping[old]}\n{content}'
    new_rows.append(dict(name=name, transform=mapping['2514999396228224437'],
                         button=mapping['7697728718566474877'], text=mapping['2175113789463330981']))

root = ub['4539244510183142245'][1]
insert = ''.join('  - {fileID: '+row['transform']+'}\n' for row in new_rows)
root = root.replace('  - {fileID: 677836894188170398}', insert+'  - {fileID: 677836894188170398}')
ui = replace_block(ui, '4539244510183142245', root)
manager = ub['5610409019247471128'][1]
for field, row in zip(['headStyleText', 'eyebrowStyleText', 'facialHairStyleText'], new_rows):
    manager += f'  {field}: {{fileID: {row["text"]}}}\n'
ui = replace_block(ui, '5610409019247471128', manager)

navigation = [('3528163364636722021','6601420142542020256'),
              ('2514999396228224437','7697728718566474877')] + [
              (row['transform'], row['button']) for row in new_rows] + [
              ('677836894188170398','8031168969483916870')]
for i, (tr, button) in enumerate(navigation):
    current = blocks(ui)
    content = current[button][1]
    up = navigation[i-1][1] if i else '0'
    down = navigation[i+1][1] if i+1 < len(navigation) else '0'
    nav = (f'  m_Navigation:\n    m_Mode: 4\n    m_WrapAround: 0\n'
           f'    m_SelectOnUp: {{fileID: {up}}}\n    m_SelectOnDown: {{fileID: {down}}}\n'
           '    m_SelectOnLeft: {fileID: 0}\n    m_SelectOnRight: {fileID: 0}\n')
    content = re.sub(r'  m_Navigation:\n.*?(?=  m_Transition:)', nav, content, flags=re.S)
    ui = replace_block(ui, button, content)
    # Match VerticalLayoutGroup's padding and spacing in the serialized layout too.
    content = current[tr][1]
    content = re.sub(r'^  m_AnchoredPosition: .*$',
        f'  m_AnchoredPosition: {{x: 300, y: {-132.15755-i*94.3151:.5f}}}', content, flags=re.M)
    ui = replace_block(ui, tr, content)
write(ui_path, ui)
print(f'Configured {len(hair_names)} hair, {len(head_names)} paired faces, '
      f'{len(male_brows)}/{len(female_brows)} eyebrows, {len(beards)} beards; 3 UI rows added.')
