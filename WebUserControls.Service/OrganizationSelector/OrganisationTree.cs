using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using WebUserControls.Infrastructure.Configuration;
using WebUserControls.Service.BasicService;
using SqlServerDataAdapter;

namespace WebUserControls.Service.OrganizationSelector
{
    public class OrganisationTree
    {
        private static readonly string _connStr = ConnectionStringFactory.NXJCConnectionString;
        private static readonly ISqlServerDataFactory _dataFactory = new SqlServerDataFactory(_connStr);
        private static readonly BasicDataHelper _dataHelper = new BasicDataHelper(_connStr);

        public static DataTable GetOrganisationTree(List<string> myOrganizationsId, string myType, List<string> myOrganizationTypeItems, int myLeveDepth, string myLeafLevelType, bool myEnabled)
        {
            string m_Enabled = myEnabled == true ? "1" : "0";

            string m_Sql = @"SELECT distinct A.OrganizationID as OrganizationId
                                  ,A.Name as Name
                                  ,A.LevelCode as LevelCode
                                  ,A.[Type] as OrganizationType
                                  ,A.LevelType as LevelType
                              FROM system_Organization A, system_Organization B, system_Organization C
                              where B.[Enabled] = {1} 
                              and (B.LevelCode like C.LevelCode + '%' or CHARINDEX(B.LevelCode, C.LevelCode) > 0) 
                              {0}
                              and CHARINDEX(A.LevelCode, B.LevelCode) > 0
                              order by A.LevelCode";
            string m_SqlCondition = "";                 //数据数据授权
            string m_SqlType = "";                      //设置用户选择的类型
            string m_SqlTypeItemsCondition = "";        //系统设定可以选择的类型    
            string m_SqlLevelDepth = "";                //系统设定显示的层次深度
            string m_SqlLeafLevelType = "";             //系统设定叶子节点类型
            /////////////////////////////数据授权////////////////////////////
            if (myOrganizationsId != null)                //数据授权约束
            {
                for (int i = 0; i < myOrganizationsId.Count; i++)
                {
                    if (i == 0)
                    {
                        m_SqlCondition = "'" + myOrganizationsId[i] + "'";
                    }
                    else
                    {
                        m_SqlCondition = m_SqlCondition + ",'" + myOrganizationsId[i] + "'";
                    }
                }
            }
            if (m_SqlCondition != "")
            {
                m_SqlCondition = " and C.OrganizationID in (" + m_SqlCondition + ")";
            }
            else
            {
                m_SqlCondition = " and C.OrganizationID <> C.OrganizationID";
            }
            /////////////////////////////////////////////////////////////////
            //////////////////////////设置可以显示的产线类型/////////////////
            if (myOrganizationTypeItems != null)
            {
                string m_ConditionTemp = " and B.Type in ({0})";
                for (int i = 0; i < myOrganizationTypeItems.Count; i++)
                {
                    if (i == 0)
                    {
                        m_SqlTypeItemsCondition = m_SqlTypeItemsCondition + "'" + myOrganizationTypeItems[i] + "'";
                    }
                    else
                    {
                        m_SqlTypeItemsCondition = m_SqlTypeItemsCondition + ",'" + myOrganizationTypeItems[i] + "'";
                    }
                }
                if (m_SqlTypeItemsCondition != "")
                {
                    m_SqlTypeItemsCondition = string.Format(m_ConditionTemp, m_SqlTypeItemsCondition);
                }
            }
            ///////////////////////////////////////////////////////////////////
            //////////////////////////设置当前用户选择的产线类型///////////////////
            if (myType != "")
            {
                m_SqlType = string.Format(" and B.Type = '{0}'", myType);
            }
            //////////////////////////////////////////////////////////////////////
            //////////////////////////设置层次深度//////////////////////////
            if (myLeveDepth > 1)
            {
                m_SqlLevelDepth = string.Format(" and len(B.LevelCode) <= {0}", myLeveDepth.ToString());
            }                
            ////////////////////////////////////////////////////////////////
            ////////////////////////////叶子节点类型///////////////////////
            if (myLeafLevelType != "")
            {
                m_SqlLeafLevelType = string.Format(" and B.LevelType = '{0}'", myLeafLevelType);
            }
            /////////////////////////////////////////////////////////
            m_Sql = string.Format(m_Sql, m_SqlCondition + m_SqlType + m_SqlTypeItemsCondition + m_SqlLevelDepth + m_SqlLeafLevelType, m_Enabled);

            try
            {
                DataTable m_Result = _dataFactory.Query(m_Sql);
                return m_Result;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static DataTable GetProductionLineType(List<string> myOrganizationTypeItems, int myLeveDepth, string myLeafLevelType, bool myEnabled)
        {
            string m_Enabled = myEnabled == true ? "1" : "0";
            string m_Sql = @"SELECT distinct B.[Type] as ProductionLineId, 
                                   B.Type as ProductionLineText 
                              FROM system_Organization B
                              where B.[Enabled] = {1}
                              and B.LevelType = 'ProductionLine' 
                              {0}
                              order by B.Type";

            string m_SqlTypeItemsCondition = "";        //系统设定可以选择的类型    
            string m_SqlLevelDepth = "";                //系统设定显示的层次深度
            string m_SqlLeafLevelType = "";             //系统设定叶子节点类型
            //////////////////////////设置可以显示的产线类型/////////////////
            if (myOrganizationTypeItems != null)
            {
                string m_ConditionTemp = " and B.Type in ({0})";
                for (int i = 0; i < myOrganizationTypeItems.Count; i++)
                {
                    if (i == 0)
                    {
                        m_SqlTypeItemsCondition = m_SqlTypeItemsCondition + "'" + myOrganizationTypeItems[i] + "'";
                    }
                    else
                    {
                        m_SqlTypeItemsCondition = m_SqlTypeItemsCondition + ",'" + myOrganizationTypeItems[i] + "'";
                    }
                }
                if (m_SqlTypeItemsCondition != "")
                {
                    m_SqlTypeItemsCondition = string.Format(m_ConditionTemp, m_SqlTypeItemsCondition);
                }
            }
            ///////////////////////////////////////////////////////////////////
            //////////////////////////设置层次深度//////////////////////////
            if (myLeveDepth > 1)
            {
                m_SqlLevelDepth = string.Format(" and len(B.LevelCode) <= {0}", myLeveDepth.ToString());
            }
            ////////////////////////////////////////////////////////////////
            ////////////////////////////叶子节点类型///////////////////////
            if (myLeafLevelType != "")
            {
                m_SqlLeafLevelType = string.Format(" and B.LevelType = '{0}'", myLeafLevelType);
            }
            /////////////////////////////////////////////////////////
            try
            {
                m_Sql = string.Format(m_Sql, m_SqlTypeItemsCondition + m_SqlLevelDepth + m_SqlLeafLevelType, m_Enabled);
                DataTable m_Result = _dataFactory.Query(m_Sql);
                return m_Result;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static List<string> GetOrganisationLevelCodeById(List<string> myOrganisationIdList)
        {
            return _dataHelper.GetOrganisationLevelCodeById(myOrganisationIdList);
        }
    }
}
