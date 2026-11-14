import json
import re
import uuid
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
ASSETS = ROOT / 'DEMO RING/Assets'
ARMOR = ASSETS / 'Data/Items/Armor'
PRESETS = ASSETS / 'Imports/PolygonFantasyHeroCharacters/Prefabs/Characters_Presets'
MODEL_SCRIPT = '277b632150c4ef2468315f6f32662ca5'
FIELDS = ['itemWeight', 'physicalDamageAbsorption', 'magicDamageAbsorption',
          'fireDamageAbsorption', 'lightningDamageAbsorption', 'holyDamageAbsorption',
          'immunity', 'robustness', 'focus', 'vitality', 'poise']

# Weight, standard physical, magic, fire, lightning, holy, immunity,
# robustness, focus, vitality, poise. Only fields supported by ArmorItem.
STATS = {
    'Archer': {
        'Head': [3, 2.8, 3.1, 3.4, 3.6, 3.1, 30, 14, 14, 16, 4],
        'Body': [7.7, 8, 10.2, 10.2, 10.9, 9.5, 52, 32, 54, 54, 15],
        'Hand': [1.7, 1.5, 1.9, 1.9, 2.1, 1.7, 15, 8, 17, 17, 2],
        'Leg': [4.4, 4, 5.4, 5.4, 5.8, 5, 34, 20, 35, 35, 8],
    },
    'Berserker': {
        'Head': [4, 4.6, 3.1, 3.6, 2.8, 2.8, 14, 23, 9, 9, 7],
        'Body': [10.6, 13.5, 10.2, 10.9, 8.8, 8.8, 35, 57, 23, 23, 22],
        'Hand': [3.5, 3.3, 2.5, 2.7, 2.1, 2.1, 12, 19, 8, 8, 5],
        'Leg': [5.7, 7.4, 5, 5.8, 4.5, 4.5, 20, 34, 13, 13, 12],
    },
}
REFERENCES = {
    'Archer': ['Bandit_Mask', 'Bandit_Garb', 'Bandit_Manchettes', 'Bandit_Boots'],
    'Berserker': ['Vagabond_Knight_Helm', 'Vagabond_Knight_Armor',
                  'Vagabond_Knight_Gauntlets', 'Vagabond_Knight_Greaves'],
}
# The enum and the runtime collection, rather than the source prefab's parent
# labels, determine the slot. The source labels for male legs are reversed.
MODEL_RULES = [
    (r'Chr_HeadCoverings_Base_Hair_', 'Head', 'Hat', 1),
    (r'Chr_HeadCoverings_No_Hair_', 'Head', 'Hood', 2),
    (r'Chr_Torso_Male_', 'Body', 'Armor', 5),
    (r'Chr_BackAttachment_', 'Body', 'Armor Back', 6),
    (r'Chr_ShoulderAttachRight_', 'Body', 'Armor Shoulder Right', 7),
    (r'Chr_ArmUpperRight_Male_', 'Body', 'Armor Upper Arm Right', 8),
    (r'Chr_ElbowAttachRight_', 'Body', 'Armor Elbow Right', 9),
    (r'Chr_ArmLowerRight_Male_', 'Hand', 'Gauntlets Lower Arm Right', 10),
    (r'Chr_HandRight_Male_', 'Hand', 'Gauntlets Hand Right', 11),
    (r'Chr_ShoulderAttachLeft_', 'Body', 'Armor Shoulder Left', 12),
    (r'Chr_ArmUpperLeft_Male_', 'Body', 'Armor Upper Arm Left', 13),
    (r'Chr_ElbowAttachLeft_', 'Body', 'Armor Elbow Left', 14),
    (r'Chr_ArmLowerLeft_Male_', 'Hand', 'Gauntlets Lower Arm Left', 15),
    (r'Chr_HandLeft_Male_', 'Hand', 'Gauntlets Hand Left', 16),
    (r'Chr_Hips_Male_', 'Leg', 'Greaves Hips', 17),
    (r'Chr_HipsAttachment_', 'Leg', 'Greaves Hips Attachment', 18),
    (r'Chr_LegRight_Male_', 'Leg', 'Greaves Leg Right', 19),
    (r'Chr_KneeAttachRight_', 'Leg', 'Greaves Knee Right', 20),
    (r'Chr_LegLeft_Male_', 'Leg', 'Greaves Leg Left', 21),
    (r'Chr_KneeAttachLeft_', 'Leg', 'Greaves Knee Left', 22),
]


def write(path, text):
    path.write_text(text, encoding='utf-8', newline='\n')


def ensure_meta(path, folder=False):
    meta = Path(str(path) + '.meta')
    if meta.exists():
        return re.search(r'^guid: (\w+)', meta.read_text(), re.M)[1]
    guid = uuid.uuid4().hex
    write(meta, 'fileFormatVersion: 2\nguid: ' + guid + '\n' + (
        'folderAsset: yes\nDefaultImporter:\n' if folder else 'NativeFormatImporter:\n'
    ) + '  externalObjects: {}\n' + ('' if folder else '  mainObjectFileID: 11400000\n')
        + '  userData:\n  assetBundleName:\n  assetBundleVariant:\n')
    return guid


def folder(path):
    path.mkdir(exist_ok=True)
    ensure_meta(path, True)


def read_prefab(path):
    text = path.read_text(encoding='utf-8-sig')
    names = set()
    active = set()
    objects = {}
    rendered = set()
    for doc in re.split(r'^--- !u!', text, flags=re.M)[1:]:
        kind, file_id = re.match(r'(\d+) &(-?\d+)', doc).groups()
        if kind == '1':
            name = re.search(r'^  m_Name: (.*)$', doc, re.M)[1]
            enabled = re.search(r'^  m_IsActive: (\d+)', doc, re.M)[1] == '1'
            objects[file_id] = (name, enabled)
            names.add(name)
        elif kind == '137':
            rendered.add(re.search(r'm_GameObject: \{fileID: (\d+)\}', doc)[1])
    for file_id in rendered:
        name, enabled = objects[file_id]
        if enabled:
            active.add(name)
    return names, active


report = []
header = (ARMOR / '_ArmorModels/Knight Set/Knight Armor.asset').read_text().split('  m_Name:')[0]
for set_name, number in [('Archer', 58), ('Berserker', 69)]:
    names, active = read_prefab(PRESETS / f'Chr_FantasyHero_Preset_{number}.prefab')
    item_folder = ARMOR / f'{set_name} Set'
    model_folder = ARMOR / '_ArmorModels' / ("Archer's Set" if set_name == 'Archer' else 'Berserker Set')
    folder(item_folder)
    folder(model_folder)
    # Finish the existing empty placeholder using its original asset GUID.
    placeholder = model_folder / 'Archer Armor Shoulder Right.asset'
    renamed = model_folder / 'Archer Armor Upper Arm Right.asset'
    if set_name == 'Archer' and placeholder.exists():
        old = placeholder.read_text()
        assert not re.search(r'equipmentModelName: \S+', old)
        assert not renamed.exists()
        placeholder.rename(renamed)
        Path(str(placeholder) + '.meta').rename(Path(str(renamed) + '.meta'))
    models_by_slot = {slot: [] for slot in ['Head', 'Body', 'Hand', 'Leg']}
    for pattern, slot, label, enum in MODEL_RULES:
        matching = sorted(name for name in active if re.match(pattern, name))
        assert len(matching) <= 1, (set_name, pattern, matching)
        if not matching:
            continue
        male = matching[0]
        female = male.replace('_Male_', '_Female_')
        assert female in names, female
        name = set_name + ' ' + label
        path = model_folder / (name + '.asset')
        # Preserve the existing Back asset, which is already configured correctly.
        text = (header + f'  m_Name: {name}\n  m_EditorClassIdentifier:\n'
                + f'  equipmentModelType: {enum}\n  equipmentModelName: {male}\n'
                + f'  femaleEquipmentModelName: {female}\n')
        if not path.exists() or any(line not in path.read_text() for line in [
                f'  equipmentModelType: {enum}', f'  equipmentModelName: {male}',
                f'  femaleEquipmentModelName: {female}']):
            write(path, text)
        guid = ensure_meta(path)
        model = {'path': path.relative_to(ROOT / 'DEMO RING').as_posix(),
                 'guid': guid, 'type': enum, 'male': male, 'female': female}
        models_by_slot[slot].append(model)

    for slot, template_name, suffix in [
            ('Head', 'Knight Helm', 'Hat' if set_name == 'Archer' else 'Hood'),
            ('Body', 'Knight Armor', 'Armor'),
            ('Hand', 'Knight Gauntlets', 'Gauntlets'),
            ('Leg', 'Knight Greaves', 'Greaves')]:
        name = set_name + ' ' + suffix
        text = (ARMOR / "Knight's Set" / (template_name + '.asset')).read_text()
        text = re.sub(r'^  (m_Name|itemName): .*$', lambda m: '  ' + m[1] + ': ' + name,
                      text, flags=re.M)
        description = {
            'Head': 'A light brimmed hat worn by archers.' if set_name == 'Archer' else 'A battle hood worn by berserkers.',
            'Body': 'Supple leather armor with a quiver, made for mobile archers.' if set_name == 'Archer' else 'Battle-worn armor with asymmetric arm guards and a back attachment.',
            'Hand': 'Light arm guards and gloves that leave room for a steady draw.' if set_name == 'Archer' else 'Sturdy bracers and gloves made for close combat.',
            'Leg': 'Flexible leggings and boots suited to moving between firing positions.' if set_name == 'Archer' else 'Layered leg armor with a belt attachment and a reinforced left knee.',
        }[slot]
        text = re.sub(r'  itemDescription:.*?(?=^  itemID:)',
                      f'  itemDescription: {description}\n', text, flags=re.S | re.M)
        # IDs are allocated by WorldItemDatabase.Awake, as for other new assets.
        text = re.sub(r'^  itemID: .*$', '  itemID: 0', text, flags=re.M)
        stats = dict(zip(FIELDS, STATS[set_name][slot]))
        for field, value in stats.items():
            text = re.sub(r'^  ' + field + r': .*$', f'  {field}: {value}', text, flags=re.M)
        text = text.split('  equipmentModels:')[0] + '  equipmentModels:\n'
        models = sorted(models_by_slot[slot], key=lambda m: m['type'])
        assert models, (set_name, slot)
        for model in models:
            text += '  - {fileID: 11400000, guid: ' + model['guid'] + ', type: 2}\n'
        if slot == 'Head':
            text += f'  headEquipmentType: {1 if set_name == "Archer" else 2}\n'
        path = item_folder / (name + '.asset')
        assert not path.exists(), path
        write(path, '\n'.join(line.rstrip() for line in text.splitlines()) + '\n')
        guid = ensure_meta(path)
        report.append({'set': set_name, 'preset': number, 'slot': slot,
                       'path': path.relative_to(ROOT / 'DEMO RING').as_posix(),
                       'guid': guid, 'stats': stats, 'models': models,
                       'balanceSource': 'https://wiki.eso-ui.com/index.php/Elden_Ring:'
                           + REFERENCES[set_name][['Head', 'Body', 'Hand', 'Leg'].index(slot)]})

write(ROOT / 'work/armor_sets_manifest.json', json.dumps(report, indent=2) + '\n')
print(f'Created {len(report)} equipment items and configured {sum(len(i["models"]) for i in report)} models.')
for item in report:
    print(item['path'], len(item['models']), 'models', 'weight', item['stats']['itemWeight'])
