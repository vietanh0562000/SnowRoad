namespace HoleBox
{
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class ColorDropdown : TMP_Dropdown
    {
        private const int bgIndex = 0;

        private int dataIndex = 0;

        protected override GameObject CreateDropdownList(GameObject template)
        {
            dataIndex = 0;
            return base.CreateDropdownList(template);
        }

        protected override DropdownItem CreateItem(DropdownItem itemTemplate)
        {
            var item = base.CreateItem(itemTemplate);

            var bg = item.transform.GetChild(bgIndex);

            var image = bg.GetComponent<Image>();

            var data = this.options[dataIndex];

            image.enabled = true;
            image.color   = data.color;
            dataIndex++;

            return item;
        }
    }
}