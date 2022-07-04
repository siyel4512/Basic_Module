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

    // 프로퍼티
    public string CodeName => codeName;
    public string DisplayName => displayName;

    #region Operator

    // Category는 주로 codeName비교를 하는 경우가 많은데
    // 편의성을 위해서 Category와 문자열을 바로 비교할 수 있도록 비교 연산자를 추가해 줌
    // 참조 블로그 https://rito15.github.io/posts/cs-why-should-inherit-iequatable/

    public bool Equals(Category other)
    {
        // 값이 없을 경우
        if (other is null)
            return false;

        // 값이 같을 경우
        if (ReferenceEquals(other, this))
            return true;

        // 값이 다른 경우
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

        // lhs, rhs 둘다 null이 아니라면 lhs의 CodeName이 같거나
        return lhs.CodeName == rhs || lhs.DisplayName == rhs;
    }

    public static bool operator !=(Category lhs, string rhs) => !(lhs == rhs);
    // category.CodeName == "Kill" x
    // category == "Kill"
    #endregion


}
