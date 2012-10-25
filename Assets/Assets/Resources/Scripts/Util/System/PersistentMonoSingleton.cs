using UnityEngine;
using System.Reflection;
using System;
using System.Collections.Generic;


struct PlayerPrefEntry{
	public PlayerPrefEntry (string attributeName, string fieldName, object fieldValue,Type fieldType)
	{
		this.attributeName = attributeName;
		this.fieldName = fieldName;
		this.fieldValue = fieldValue;
		this.fieldType = fieldType;
	}

	public string attributeName;
	public string fieldName;
	public object fieldValue;
	public Type fieldType;
}



public abstract class PersistentMonoSingleton<T> : MonoSingleton<T> where T:MonoSingleton<T>
{		
   	BindingFlags BINDING_FLAGS=BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
	private static Type EXPECTED_TYPE=typeof(Persistent);
	
	
	public override void Init(){		
		Attribute attr = System.Attribute.GetCustomAttribute(typeof(T),EXPECTED_TYPE);
		if (attr!=null && ((Persistent)attr).updateOnInstantiation){
			readPropertiesFromPlayerPrefs();	
		}			
	}
	
	
	private string getPreferenceKey(string classAttribute, PlayerPrefEntry ppe){
		return classAttribute + "." + 
			(ppe.attributeName.Equals("") ? ppe.fieldName : ppe.attributeName);	
	}
	
	string getClassAtributeProperty()
	{
		string result="";
		Attribute attr = System.Attribute.GetCustomAttribute(typeof(T),EXPECTED_TYPE);
		if (attr!=null)
			result=((Persistent)attr).name;
		return result;
	}
	
	private List<PlayerPrefEntry> getFieldsEntryFromClass(){
		List<PlayerPrefEntry> result=new List<PlayerPrefEntry>();
		MemberInfo[] infos=typeof(T).GetMembers(BINDING_FLAGS);
		foreach (MemberInfo info in infos) {
			object[] attributes=info.GetCustomAttributes(EXPECTED_TYPE, false);
			foreach (object attribute in attributes) {
				
				Persistent pa=(Persistent) attribute;
				string attributeName=pa.name;
				string fieldName=info.Name;
				
				FieldInfo fieldInfo = typeof(T).GetField(fieldName, BINDING_FLAGS);				
				object fieldValue = fieldInfo.GetValue(this);
				Type fieldType= fieldInfo.FieldType;
				result.Add(new PlayerPrefEntry(attributeName, fieldName,fieldValue,fieldType));				
			}
		}
		return result;
	}
	
	
	
	public void savePropertiesToPlayerPrefs(){
		string classAtribute=getClassAtributeProperty();
		List<PlayerPrefEntry> ppe=getFieldsEntryFromClass();
		foreach (PlayerPrefEntry item in ppe) {
			string key=getPreferenceKey(classAtribute,item);
			object fieldValue=item.fieldValue;
			if (fieldValue is int){
					PlayerPrefs.SetInt(key, (int)fieldValue);
				} else if(fieldValue is float){
					PlayerPrefs.SetFloat(key, (float)fieldValue);
				} else if(fieldValue is string){
					PlayerPrefs.SetString(key, (string)fieldValue);
				} else {
					Debug.LogWarning("Cant save property, unsuported type "+item.fieldValue.GetType());
				}
		}
		
		PlayerPrefs.Save();
		
	}
	
	public void readPropertiesFromPlayerPrefs(){
		string classAtribute=getClassAtributeProperty();
		List<PlayerPrefEntry> ppe=getFieldsEntryFromClass();
		foreach (PlayerPrefEntry item in ppe) {
			
			
			string key=getPreferenceKey(classAtribute,item);				
			Type fieldType=item.fieldType;
			object newValue=new object();			
			if (fieldType== typeof(int)){
					newValue = PlayerPrefs.GetInt(key);
				} else if(fieldType== typeof(float)){
					newValue = PlayerPrefs.GetFloat(key);
				} else if(fieldType== typeof(string)){
					newValue = PlayerPrefs.GetString(key);
				} else {
					Debug.LogWarning("Cant read property, unsuported type "+item);
					continue;
				}
			FieldInfo fieldInfo = typeof(T).GetField(item.fieldName, BINDING_FLAGS);	
			if (newValue!=null)
				fieldInfo.SetValue(this,newValue);
		}
		
		PlayerPrefs.Save();
	}
	
	public void resetAllProperties(){
		PlayerPrefs.DeleteAll();
		PlayerPrefs.Save();
	}
}