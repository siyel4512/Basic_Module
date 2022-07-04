using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Category", fileName = "Category_")]
public class Category : ScriptableObject, IEquatable<Category>
{
    [SerializeField]
    private string codeName;

    [SerializeField]
    private string displayName;

    // ������Ƽ
    public string CodeName => codeName;
    public string DisplayName => displayName;

    #region Operator

    // Category�� �ַ� codeName�񱳸� �ϴ� ��찡 ������
    // ���Ǽ��� ���ؼ� Category�� ���ڿ��� �ٷ� ���� �� �ֵ��� �� �����ڸ� �߰��� ��
    // ���� ��α� https://rito15.github.io/posts/cs-why-should-inherit-iequatable/

    public bool Equals(Category other)
    {
        // ���� ���� ���
        if (other is null)
            return false;

        // ���� ���� ���
        if (ReferenceEquals(other, this))
            return true;

        // ���� �ٸ� ���
        if (GetType() != other.GetType())
            return false;

        return codeName == other.CodeName;
    }

    public override int GetHashCode() => (CodeName, DisplayName).GetHashCode();

    public override bool Equals(object other) => base.Equals(other);

    public static bool operator ==(Category lhs, string rhs)
    {
        if (lhs is null)
            return ReferenceEquals(rhs, null);

        // lhs, rhs �Ѵ� null�� �ƴ϶�� lhs�� CodeName�� ���ų�
        return lhs.CodeName == rhs || lhs.DisplayName == rhs;
    }

    public static bool operator !=(Category lhs, string rhs) => !(lhs == rhs);
    // category.CodeName == "Kill" x
    // category == "Kill"
    #endregion


}
