import hashlib
import json
import re
import uuid
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
ASSETS = ROOT / 'DEMO RING/Assets'
ITEMS = ASSETS / 'Data/Items/Weapons/Melee Weapons'
PREFABS = ASSETS / 'Data/Prefab/Items/Weapons/Melee Weapons'
ANIMATORS = ASSETS / 'Data/Animator Controller/_Override Animator'
DATABASE = ASSETS / 'Data/Prefab/World Managers/World Item Database.prefab'


def meta(path, file_id):
    guid = uuid.uuid4().hex
    Path(str(path) + '.meta').write_text(
        f'fileFormatVersion: 2\nguid: {guid}\nNativeFormatImporter:\n'
        f'  externalObjects: {{}}\n  mainObjectFileID: {file_id}\n'
        '  userData:\n  assetBundleName:\n  assetBundleVariant:\n', encoding='utf-8')
    return guid


configs = [
    dict(prefab='Weapon_Axe_01', asset='Battle Axe', animator='Axe', display='Battle Axe',
         weaponClass=5, itemWeight=4.5, strengthREQ=12, dexterityREQ=8,
         physicalDamage=128, physicalDamageAbsorption=47, magicalDamageAbsorption=31,
         fireDamageAbsorption=31, lightningDamageAbsorption=31, holyDamageAbsorption=31,
         staminaAbsorption=31, poiseDamage=30, basicStaminaCost=24,
         description='A versatile battle axe with a heavy blade. Strong swings reward careful timing.',
         source='https://wiki.eso-ui.com/index.php/Elden_Ring:Battle_Axe'),
    dict(prefab='Weapon_Great_Axe_01', asset='Great Axe', animator='Great Axe', display='Greataxe',
         weaponClass=6, itemWeight=13, strengthREQ=30, dexterityREQ=8,
         physicalDamage=151, physicalDamageAbsorption=69, magicalDamageAbsorption=37,
         fireDamageAbsorption=37, lightningDamageAbsorption=37, holyDamageAbsorption=37,
         staminaAbsorption=45, poiseDamage=40, basicStaminaCost=32,
         description='A heavy greataxe for powerful warriors. Its weight favors deliberate, committed attacks.',
         source='https://wiki.eso-ui.com/index.php/Elden_Ring:Greataxe'),
]
database_text = DATABASE.read_text(encoding='utf-8-sig')
lookup = {}
for p in ASSETS.rglob('*.asset.meta'):
    match = re.search(r'^guid: (\w+)', p.read_text(encoding='utf-8-sig'), re.M)
    if match:
        lookup[match[1]] = Path(str(p)[:-5])
registered = {}
weapons = re.search(r'^  weapons:\n(.*?)(?=^  headEquipment:)', database_text, re.S | re.M)[1]
next_id = max(int(re.search(r'^  itemID: (\d+)', lookup[g].read_text(), re.M)[1])
              for g in re.findall(r'guid: (\w+)', weapons)) + 1
for guid in re.findall(r'  - \{fileID: 11400000, guid: (\w+), type: 2\}', database_text):
    item_path = lookup[guid]
    registered[guid] = int(re.search(r'^  itemID: (\d+)', item_path.read_text(), re.M)[1])

result = []
for offset, config in enumerate(configs):
    prefab_path = PREFABS / (config['prefab'] + '.prefab')
    prefab = prefab_path.read_text(encoding='utf-8-sig')
    transforms_before = re.findall(r'^--- !u!4 .*?(?=^---|\Z)', prefab, re.S | re.M)
    prefab_guid = re.search(r'^guid: (\w+)', Path(str(prefab_path) + '.meta').read_text(), re.M)[1]
    root_go = None
    damage_component = None
    for doc in re.split(r'^--- !u!', prefab, flags=re.M)[1:]:
        kind, fid = re.match(r'(\d+) &(-?\d+)', doc).groups()
        if kind == '1' and f'  m_Name: {config["prefab"]}\n' in doc:
            root_go = fid
        if kind == '114' and 'guid: e8f7e699a11501e4386d763342df3e0a' in doc:
            damage_component = fid
    assert root_go and damage_component
    prefab = prefab.replace('meleeWeaponDamageCollider: {fileID: 0}',
                            'meleeWeaponDamageCollider: {fileID: ' + damage_component + '}')
    assert transforms_before == re.findall(r'^--- !u!4 .*?(?=^---|\Z)', prefab, re.S | re.M)
    prefab_path.write_text(prefab, encoding='utf-8', newline='\n')
    animator_path = ANIMATORS / (config['animator'] + '.overrideController')
    assert not animator_path.exists(), animator_path
    animator = (ANIMATORS / 'Straight Sword.overrideController').read_text()
    animator = re.sub(r'^  m_Name: .*$', '  m_Name: ' + config['animator'], animator, flags=re.M)
    animator_path.write_text(animator, encoding='utf-8', newline='\n')
    animator_guid = meta(animator_path, 22100000)

    path = ITEMS / (config['asset'] + '.asset')
    assert not path.exists(), path
    text = (ITEMS / 'Broad Sword.asset').read_text(encoding='utf-8-sig')
    values = {k: v for k, v in config.items() if isinstance(v, (int, float))}
    values.update(m_Name=config['asset'], itemName=config['display'],
                  itemDescription=config['description'], itemID=next_id + offset)
    for field, value in values.items():
        assert re.search(r'^  ' + field + ': .*$', text, re.M), field
        text = re.sub(r'^  ' + field + r': .*$', f'  {field}: {value}', text, flags=re.M)
    text = re.sub(r'^  weaponAnimator: .*$',
                  '  weaponAnimator: {fileID: 22100000, guid: ' + animator_guid + ', type: 2}', text, flags=re.M)
    text = re.sub(r'^  weaponModel:.*?(?=^  strengthREQ:)',
                  '  weaponModel: {fileID: ' + root_go + ', guid: ' + prefab_guid + ', type: 3}\n', text, flags=re.S | re.M)
    path.write_text('\n'.join(line.rstrip() for line in text.splitlines()) + '\n', encoding='utf-8')
    guid = meta(path, 11400000)
    config.update(path=path.relative_to(ROOT / 'DEMO RING').as_posix(), guid=guid,
                  itemID=next_id + offset, prefabPath=prefab_path.relative_to(ROOT / 'DEMO RING').as_posix(),
                  animatorPath=animator_path.relative_to(ROOT / 'DEMO RING').as_posix(),
                  transformHashes=[hashlib.sha256(t.encode()).hexdigest() for t in transforms_before])
    result.append(config)
    print(config['display'], 'ID', config['itemID'], 'physical', config['physicalDamage'], 'weight', config['itemWeight'])

refs = ''.join('  - {fileID: 11400000, guid: ' + c['guid'] + ', type: 2}\n' for c in result)
database_text = re.sub(r'(^  weapons:\n(?:  - .*\n)*)', lambda m: m[1] + refs, database_text, flags=re.M)
DATABASE.write_text(database_text, encoding='utf-8', newline='\n')
(ROOT / 'work/axes_manifest.json').write_text(json.dumps({'weapons': result, 'previousIDs': registered}, indent=2), encoding='utf-8')
print('Registered both weapons, linked damage components, and preserved all prefab transforms.')
