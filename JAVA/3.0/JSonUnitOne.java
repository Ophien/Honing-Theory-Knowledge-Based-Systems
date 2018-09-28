/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package honing.java;

import java.util.ArrayList;

/**
 *
 * @author Alysson
 */
public class JSonUnitOne {

    String card_name;
    String standardized_card_name;
    String mana;
    String attack;
    String health;
    String mf0;
    String mf1;
    String mf2;
    String mf3;
    String mf4;
    String mf5;
    String mf6;

    public ArrayList<String> getMF() {
        ArrayList<String> mfs = new ArrayList<>();

        if (mf0 != null) {
            mfs.add(mf0);
        }

        if (mf1 != null) {
            mfs.add(mf1);
        }

        if (mf2 != null) {
            mfs.add(mf2);
        }

        if (mf3 != null) {
            mfs.add(mf3);
        }

        if (mf4 != null) {
            mfs.add(mf4);
        }

        if (mf5 != null) {
            mfs.add(mf5);
        }
        if (mf6 != null) {
            mfs.add(mf6);
        }

        return mfs;
    }
    
    public String getNormalizedName(){
     return   standardized_card_name;
    }

}